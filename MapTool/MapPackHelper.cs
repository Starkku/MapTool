/*
 * Copyright 2017-2020 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

using System;
using MapTool.Encodings;

namespace MapTool
{
    /// <summary>
    /// Helper class for decompressing & compressing packed data sections in map files.
    /// </summary>
    public class MapPackHelper
    {
        /// <summary>
        /// Decompresses map pack data.
        /// </summary>
        /// <param name="dataSource">Array of compressed map pack data.</param>
        /// <param name="dataDest">Array to write decompressed map pack data to.</param>
        /// <param name="totalDecompressedSize">Set to total amount of bytes decompressed.</param>
        /// <param name="useLCW">If set to true, uses LCW instead of LZO.</param>
        /// <returns>True if successfully decompressed all of the data, otherwise false.</returns>
        public static unsafe bool Decompress(byte[] dataSource, byte[] dataDest, out int totalDecompressedSize, bool useLCW = false)
        {
            fixed (byte* pRead = dataSource, pWrite = dataDest)
            {
                byte* read = pRead, write = pWrite;
                byte* writeEnd = write + dataDest.Length;
                int readBytes = 0;
                int writtenBytes = 0;

                while (write < writeEnd)
                {
                    ushort sizeCompressed = *(ushort*)read;
                    read += 2;
                    uint sizeUncompressed = *(ushort*)read;
                    read += 2;
                    readBytes += 4;

                    if (sizeCompressed == 0 || sizeUncompressed == 0)
                        break;

                    if (readBytes + sizeCompressed > dataSource.Length ||
                        writtenBytes + sizeUncompressed > dataDest.Length)
                    {
                        totalDecompressedSize = writtenBytes;
                        return false;
                    }

                    if (useLCW)
                        LCW.DecodeInto(read, write);
                    else
                        MiniLZO.Decompress(read, sizeCompressed, write, ref sizeUncompressed);

                    read += sizeCompressed;
                    write += sizeUncompressed;
                    readBytes += sizeCompressed;
                    writtenBytes += (int)sizeUncompressed;
                }
                totalDecompressedSize = writtenBytes;
            }
            return true;
        }

        /// <summary>
        /// Compresses map pack data.
        /// </summary>
        /// <param name="dataSource">Array of map pack data to compress.</param>
        /// <param name="useLCW">If set to true, uses LCW instead of LZO.</param>
        /// <returns>Compressed map pack data.</returns>
        public static byte[] Compress(byte[] dataSource, bool useLCW = false)
        {
            var dataDest = new byte[dataSource.Length * 2];
            int writtenBytes = 0, readBytes = 0;

            while (readBytes < dataSource.Length)
            {
                short nextBlockSize = (short)Math.Min(dataSource.Length - readBytes, 8192);
                byte[] blockIn = new byte[nextBlockSize];
                Array.Copy(dataSource, readBytes, blockIn, 0, nextBlockSize);
                readBytes += nextBlockSize;
                byte[] blockOut = useLCW ? LCW.Encode(blockIn) : MiniLZO.Compress(blockIn);
                uint blockOutSize = (ushort)blockOut.Length;

                Array.Copy(BitConverter.GetBytes(blockOutSize), 0, dataDest, writtenBytes, 2);
                writtenBytes += 2;
                Array.Copy(BitConverter.GetBytes(nextBlockSize), 0, dataDest, writtenBytes, 2);
                writtenBytes += 2;
                Array.Copy(blockOut, 0, dataDest, writtenBytes, blockOut.Length);
                writtenBytes += blockOut.Length;
            }

            Array.Resize(ref dataDest, writtenBytes);
            return dataDest;
        }
    }
}