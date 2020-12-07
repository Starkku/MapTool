/*
* C# port of lcw.cpp originally authored by OmniBlade for Chronoshift project.
* https://github.com/TheAssemblyArmada/Chronoshift/blob/c91eb086587b895bff7fb4e53a1d31992b7bad18/src/game/common/lcw.cpp
* Original copyright notice is included below.
*/

/*
* Chronoshift is free software: you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation, either version
* 2 of the License, or (at your option) any later version.
* A full copy of the GNU General Public License can be found in
* LICENSE
*/

namespace MapTool.Logic
{
    /// <summary>
    /// Implementation of LCW, a custom compression format used in many Westwood games.
    /// </summary>
    internal static class LCW
    {
        ////////////////////////////////////////////////////////////////////////////////
        //  Notes
        ////////////////////////////////////////////////////////////////////////////////
        //
        // LCW streams should always start and end with the fill command (& 0x80) though
        // the decompressor doesn't strictly require that it start with one the ability
        // to use the offset commands in place of the RLE command early in the stream
        // relies on it. Streams larger than 64k that need the relative versions of the
        // 3 and 5 byte commands should start with a null byte before the first 0x80
        // command to flag that they are relative compressed.
        //
        // LCW uses the following rules to decide which command to use:
        // 1. Runs of the same colour should only use 4 byte RLE command if longer than
        //    64 bytes. 2 and 3 byte offset commands are more efficient otherwise.
        // 2. Runs of less than 3 should just be stored as is with the one byte fill
        //    command.
        // 3. Runs greater than 10 or if the relative offset is greater than
        //    4095 use an absolute copy. Less than 64 bytes uses 3 byte command, else it
        //    uses the 5 byte command.
        // 4. If Absolute rule isn't met then copy from a relative offset with 2 byte
        //    command.
        //
        // Absolute LCW can efficiently compress data that is 64k in size, much greater
        // and relative offsets for the 3 and 5 byte commands are needed.
        //
        ////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Decompresses data in the proprietary LCW format used in many games developed by Westwood Studios.
        /// Warning: Data starting with a 0 will be treated as being compressed with relative offsets rather than absolute to start of buffer.
        /// </summary>
        /// <param name="source">Compressed data.</param>
        /// <param name="dest">Decompressed data.</param>
        /// <param name="bytes">Number of bytes to decompress.</param>
        /// <returns>Number of decompressed bytes.</returns>
        public unsafe static int Decompress(byte* source, byte* dest, uint bytes)
        {
            if (source == null || dest == null)
                return 0;

            byte* destStart = dest;
            byte* destEnd = destStart + bytes;
            byte* destP = destStart;
            byte* sourceP = source;

            // If first byte is 0, the all offsets are relative to current position. Otherwise some are absolute to the start of the
            // buffer, meaning only ~64KB can be compressed effectively. Compressor implemented in this file uses size to determine
            // compression scheme used.
            if (*sourceP == 0)
            {
                sourceP++;

                while (destP < destEnd)
                {
                    byte flag;
                    ushort copySize;
                    ushort offset;

                    flag = *sourceP++;

                    if ((flag & 0x80) > 0)
                    {
                        if ((flag & 0x40) > 0)
                        {
                            copySize = (ushort)((flag & 0x3F) + 3);
                            // long set 0b11111110
                            if (flag == 0xFE)
                            {
                                copySize = *sourceP++;
                                copySize += (ushort)((*sourceP++) << 8);

                                if (copySize > destEnd - destP)
                                    copySize = (ushort)(destEnd - destP);

                                MemSet(destP, *sourceP++, copySize);
                                destP += copySize;
                            }
                            else
                            {
                                byte* s;
                                if (flag == 0xFF)
                                {
                                    copySize = *sourceP++;
                                    copySize += (ushort)((*sourceP++) << 8);

                                    if (copySize > destEnd - destP)
                                        copySize = (ushort)(destEnd - destP);

                                    offset = *sourceP++;
                                    offset += (ushort)((*sourceP++) << 8);

                                    // extended format for VQA32 and large WSA files.
                                    s = destP - offset;

                                    for (; copySize > 0; --copySize)
                                    {
                                        *destP++ = *s++;
                                    }
                                    // short move abs 0b11??????
                                }
                                else
                                {
                                    if (copySize > destEnd - destP)
                                        copySize = (ushort)(destEnd - destP);


                                    offset = *sourceP++;
                                    offset += (ushort)((*sourceP++) << 8);

                                    // extended format for VQA32 and large WSA files.
                                    s = destP - offset;

                                    for (; copySize > 0; --copySize)
                                    {
                                        *destP++ = *s++;
                                    }
                                }
                            }
                            // short copy 0b10??????
                        }
                        else
                        {
                            if (flag == 0x80)
                            {
                                return (int)(destP - destStart);
                            }

                            copySize = (ushort)(flag & 0x3F);

                            if (copySize > destEnd - destP)
                                copySize = (ushort)(destEnd - destP);

                            for (; copySize > 0; --copySize)
                            {
                                *destP++ = *sourceP++;
                            }
                        }
                        // short move rel 0b0???????
                    }
                    else
                    {
                        copySize = (ushort)((flag >> 4) + 3);

                        if (copySize > destEnd - destP)
                            copySize = (ushort)(destEnd - destP);

                        offset = (ushort)(((flag & 0xF) << 8) + (*sourceP++));

                        for (; copySize > 0; --copySize)
                        {
                            *destP = *(destP - offset);
                            destP++;
                        }
                    }
                }
            }
            else
            {
                while (destP < destEnd)
                {
                    byte flag;
                    ushort copySize;
                    ushort offset;

                    flag = *sourceP++;

                    if ((flag & 0x80) > 0)
                    {
                        if ((flag & 0x40) > 0)
                        {
                            copySize = (ushort)((flag & 0x3F) + 3);
                            // long set 0b11111110
                            if (flag == 0xFE)
                            {
                                copySize = *sourceP++;
                                copySize += (ushort)((*sourceP++) << 8);

                                if (copySize > destEnd - destP)
                                    copySize = (ushort)(destEnd - destP);

                                MemSet(destP, *sourceP++, copySize);
                                destP += copySize;
                            }
                            else
                            {
                                byte* s;
                                // long move, abs 0b11111111
                                if (flag == 0xFF)
                                {
                                    copySize = *sourceP++;
                                    copySize += (ushort)((*sourceP++) << 8);

                                    if (copySize > destEnd - destP)
                                        copySize = (ushort)(destEnd - destP);

                                    offset = *sourceP++;
                                    offset += (ushort)((*sourceP++) << 8);
                                    s = destStart + offset;

                                    for (; copySize > 0; --copySize)
                                    {
                                        *destP++ = *s++;
                                    }
                                    // short move abs 0b11??????
                                }
                                else
                                {
                                    if (copySize > destEnd - destP)
                                        copySize = (ushort)(destEnd - destP);

                                    offset = *sourceP++;
                                    offset += (ushort)((*sourceP++) << 8);
                                    s = destStart + offset;

                                    for (; copySize > 0; --copySize)
                                    {
                                        *destP++ = *s++;
                                    }
                                }
                            }
                            // short copy 0b10??????
                        }
                        else
                        {
                            if (flag == 0x80)
                                return (int)(destP - destStart);

                            copySize = (ushort)(flag & 0x3F);

                            if (copySize > destEnd - destP)
                                copySize = (ushort)(destEnd - destP);

                            for (; copySize > 0; --copySize)
                            {
                                *destP++ = *sourceP++;
                            }
                        }
                        // short move rel 0b0???????
                    }
                    else
                    {
                        copySize = (ushort)((flag >> 4) + 3);

                        if (copySize > destEnd - destP)
                            copySize = (ushort)(destEnd - destP);

                        offset = (ushort)(((flag & 0xF) << 8) + (*sourceP++));

                        for (; copySize > 0; --copySize)
                        {
                            *destP = *(destP - offset);
                            destP++;
                        }
                    }
                }
            }

            return (int)(destP - destStart);
        }

        /// <summary>
        /// Compresses data to the proprietary LCW format used in many games developed by Westwood Studios.
        /// Warning: Worst case can have the compressed data larger than the original.
        /// </summary>
        /// <param name="source">Data to compress.</param>
        /// <param name="dest">Compressed data.</param>
        /// <param name="bytes">Number of bytes to compress.</param>
        /// <returns>Number of compressed bytes.</returns>
        public static unsafe int Compress(byte* source, byte* dest, uint bytes)
        {
            if (source == null || dest == null || bytes < 1)
                return 0;

            // Decide if we are going to do relative offsets for 3 and 5 byte commands
            bool relative = bytes > ushort.MaxValue;

            byte* sourceP = source;
            byte* destP = dest;
            byte* sourceStart = sourceP;
            byte* sourceEnd = sourceP + bytes;
            byte* destStart = destP;

            // relative LCW starts with 0 to flag as relative for decoder
            if (relative)
                *destP++ = 0;

            // Write a starting cmd1 and set bool to have cmd1 in progress
            byte* cmdOneP = destP;
            *destP++ = 0x81;
            *destP++ = *sourceP++;
            bool cmdOne = true;

            // Compress data
            while (sourceP < sourceEnd)
            {
                // Is RLE encode (4bytes) worth evaluating?
                if (sourceEnd - sourceP > 64 && *sourceP == *(sourceP + 64))
                {
                    // RLE run length is encoded as a short so max is UINT16_MAX
                    byte* RLEMax = (sourceEnd - sourceP) < ushort.MaxValue ? sourceEnd : sourceP + ushort.MaxValue;
                    byte* RLEP;

                    for (RLEP = sourceP + 1; *RLEP == *sourceP && RLEP < RLEMax; ++RLEP)
                        ;

                    ushort runLength = (ushort)(RLEP - sourceP);

                    // If run length is long enough, write the command and start loop again
                    if (runLength >= 0x41)
                    {
                        cmdOne = false;
                        *destP++ = 0xFE;
                        *destP++ = (byte)(runLength & 0xff);
                        *destP++ = (byte)(runLength >> 8);
                        *destP++ = *sourceP;
                        sourceP = RLEP;
                        continue;
                    }
                }

                // current block size for an offset copy
                int blockSize = 0;
                byte* offstart;

                // Set where we start looking for matching runs.
                if (relative)
                    offstart = (sourceP - sourceStart) < ushort.MaxValue ? sourceStart : sourceP - ushort.MaxValue;
                else
                    offstart = sourceStart;

                // Look for matching runs
                byte* offCheck = offstart;
                byte* offsetP = sourceP;

                while (offCheck < sourceP)
                {
                    // Move offchk to next matching position
                    while (offCheck < sourceP && *offCheck != *sourceP)
                        ++offCheck;

                    // If the checking pointer has reached current pos, break
                    if (offCheck >= sourceP)
                        break;

                    // find out how long the run of matches goes for
                    //<= because it can consider the current pixel as part of a run
                    int i;
                    for (i = 1; &sourceP[i] < sourceEnd; ++i)
                    {
                        if (offCheck[i] != sourceP[i])
                            break;
                    }

                    if (i >= blockSize)
                    {
                        blockSize = i;
                        offsetP = offCheck;
                    }

                    ++offCheck;
                }

                // decide what encoding to use for current run
                if (blockSize <= 2)
                {
                    // short copy 0b10??????
                    // check we have an existing 1 byte command and if its value is still
                    // small enough to handle additional bytes
                    // start a new command if current one doesn't have space or we don't
                    // have one to continue
                    if (cmdOne && *cmdOneP < 0xBF)
                    {
                        // increment command value
                        ++*cmdOneP;
                        *destP++ = *sourceP++;
                    }
                    else
                    {
                        cmdOneP = destP;
                        *destP++ = 0x81;
                        *destP++ = *sourceP++;
                        cmdOne = true;
                    }
                }
                else
                {
                    ushort offset;
                    ushort rel_offset = (ushort)(sourceP - offsetP);
                    if (blockSize > 0xA || (rel_offset > 0xFFF))
                    {
                        // write 5 byte command 0b11111111
                        if (blockSize > 0x40)
                        {
                            *destP++ = 0xFF;
                            ushort val = (ushort)blockSize;
                            *destP++ = (byte)(val & 0xff);
                            *destP++ = (byte)(val >> 8);
                            // write 3 byte command 0b11??????
                        }
                        else
                            *destP++ = (byte)((blockSize - 3) | 0xC0);

                        offset = (ushort)(relative ? rel_offset : offsetP - sourceStart);
                        // write 2 byte command? 0b0???????
                    }
                    else
                        offset = (ushort)(rel_offset << 8 | (16 * (blockSize - 3) + (rel_offset >> 8)));

                    *destP++ = (byte)(offset & 0xff);
                    *destP++ = (byte)(offset >> 8);
                    sourceP += blockSize;
                    cmdOne = false;
                }
            }

            // write final 0x80, this is why its also known as format80 compression
            *destP++ = 0x80;
            return (int)(destP - destStart);
        }

        private static unsafe void MemSet(byte* buffer, byte value, ushort size)
        {
            for (int i = 0; i < size; i++)
            {
                *(buffer + i) = value;
            }
        }
    }
}