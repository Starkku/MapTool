/*
 * Copyright 2017-2020 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 2 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

using System;

namespace MapTool.Logic
{
    /// <summary>
    /// Helper class for decompressing & compressing packed data sections in map files.
    /// </summary>
    internal class MapPackHelper
    {
        /// <summary>
        /// Parses and decompresses Base64-encoded and compressed map pack data.
        /// </summary>
        /// <param name="encodedData">Encoded & compressed map pack data as string.</param>
        /// <param name="decompressedData">Array to put the decompressed data to.</param>
        /// <param name="useLCW">If set to true, treat data as LCW-compressed instead of LZO.</param>
        /// <returns>Error message if something went wrong, otherwise null.</returns>
        public static string ParseMapPackData(string encodedData, ref byte[] decompressedData, bool useLCW = false)
        {
            byte[] compressedData;
            try
            {
                compressedData = Convert.FromBase64String(encodedData);
            }
            catch (Exception)
            {
                return "Map pack data is malformed.";
            }
            if (Decompress(compressedData, decompressedData, out _, useLCW))
            {
                return null;
            }
            else
            {
                return "Map pack data is invalid or corrupted.";
            }
        }

        /// <summary>
        /// Parses and decompresses Base64-encoded and compressed map pack data.
        /// </summary>
        /// <param name="encodedData">Array of encoded & compressed map pack data.</param>
        /// <param name="decompressedData">Array to put the decompressed data to.</param>
        /// <param name="useLCW">If set to true, treat data as LCW-compressed instead of LZO.</param>
        /// <returns>Error message if something went wrong, otherwise null.</returns>
        public static string ParseMapPackData(string[] encodedData, ref byte[] decompressedData, bool useLCW = false)
        {
            return ParseMapPackData(string.Join("", encodedData), ref decompressedData, useLCW);
        }

        /// <summary>
        /// Compresses and and Base64-encodes map pack data.
        /// </summary>
        /// <param name="mapPackData">Map pack data.</param>
        /// <param name="useLCW">If set to true, use LCW compression instead of LZO.</param>
        /// <returns>Compressed & encoded map pack data. Null if compression or encoding fails.</returns>
        public static string CompressMapPackData(byte[] mapPackData, bool useLCW = false)
        {
            bool success = Compress(mapPackData, out byte[] compressedData, out _, useLCW);

            if (!success)
                return null;

            try
            {
                return Convert.ToBase64String(compressedData);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Decompresses LZO / LCW compressed map pack data.
        /// </summary>
        /// <param name="dataSource">Array of compressed map pack data.</param>
        /// <param name="dataDest">Array to write decompressed map pack data to.</param>
        /// <param name="totalDecompressedSize">Set to total amount of bytes decompressed.</param>
        /// <param name="useLCW">If set to true, uses LCW instead of LZO.</param>
        /// <returns>True if successfully decompressed all of the data, otherwise false.</returns>
        private static unsafe bool Decompress(byte[] dataSource, byte[] dataDest, out int totalDecompressedSize, bool useLCW = false)
        {
            totalDecompressedSize = 0;

            if (dataSource == null || dataDest == null)
                return false;

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
                        LCW.Decompress(read, write, sizeUncompressed);
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
        /// Compresses map pack data using LZO or LCW compression.
        /// </summary>
        /// <param name="dataSource">Array of map pack data to compress.</param>
        /// <param name="dataDest">Array to write compressed map pack data to.</param>
        /// <param name="totalCompressedSize">Set to total amount of bytes compressed.</param>
        /// <param name="useLCW">If set to true, uses LCW instead of LZO.</param>
        /// <returns>True if successfully compressed all of the data, otherwise false.</returns>
        private static unsafe bool Compress(byte[] dataSource, out byte[] dataDest, out int totalCompressedSize, bool useLCW = false)
        {
            dataDest = new byte[dataSource.Length * 2];
            totalCompressedSize = 0;

            if (dataSource == null)
                return false;

            int writtenBytes = 0, readBytes = 0;

            int blockSize = 8192;
            int lcwWorstCaseBufferSize = blockSize + (blockSize / 128) + 1 + blockSize;

            while (readBytes < dataSource.Length)
            {
                short nextBlockSize = (short)Math.Min(dataSource.Length - readBytes, blockSize);
                byte[] blockIn = new byte[nextBlockSize];
                Array.Copy(dataSource, readBytes, blockIn, 0, nextBlockSize);
                readBytes += nextBlockSize;

                byte[] blockOut;
                uint blockOutSize;

                if (useLCW)
                {
                    blockOut = new byte[lcwWorstCaseBufferSize];
                    fixed (byte* src = blockIn, dest = blockOut)
                    {
                        blockOutSize = (uint)LCW.Compress(src, dest, (uint)nextBlockSize);
                    }
                }
                else
                {
                    blockOut = MiniLZO.Compress(blockIn);
                    blockOutSize = (ushort)blockOut.Length;
                }

                Array.Copy(BitConverter.GetBytes(blockOutSize), 0, dataDest, writtenBytes, 2);
                writtenBytes += 2;
                Array.Copy(BitConverter.GetBytes(nextBlockSize), 0, dataDest, writtenBytes, 2);
                writtenBytes += 2;
                Array.Copy(blockOut, 0, dataDest, writtenBytes, blockOutSize);
                writtenBytes += (int)blockOutSize;
            }

            totalCompressedSize = writtenBytes;
            Array.Resize(ref dataDest, writtenBytes);
            return true;
        }
    }
}