using System;
using System.IO;

namespace MapT.Utility
{

    // From OpenRA https://github.com/OpenRA/OpenRA/blob/bleed/OpenRA.Game/FileFormats/Format80.cs
    public static class Format80_Old
    {

        static void ReplicatePrevious(byte[] dest, int destIndex, int srcIndex, int count)
        {
            if (srcIndex > destIndex)
                throw new NotImplementedException(string.Format("srcIndex > destIndex  {0}  {1}", srcIndex, destIndex));

            if (destIndex - srcIndex == 1)
            {
                for (int i = 0; i < count; i++)
                    dest[destIndex + i] = dest[destIndex - 1];
            }
            else
            {
                for (int i = 0; i < count; i++)
                    dest[destIndex + i] = dest[srcIndex + i];
            }
        }

        public static int DecodeInto(byte[] src, byte[] dest)
        {
            VirtualFile ctx = new MemoryFile(src);
            int destIndex = 0;

            while (true)
            {
                byte i = ctx.ReadByte();
                if ((i & 0x80) == 0)
                {
                    // case 2
                    byte secondByte = ctx.ReadByte();
                    int count = ((i & 0x70) >> 4) + 3;
                    int rpos = ((i & 0xf) << 8) + secondByte;

                    ReplicatePrevious(dest, destIndex, destIndex - rpos, count);
                    destIndex += count;
                }
                else if ((i & 0x40) == 0)
                {
                    // case 1
                    int count = i & 0x3F;
                    if (count == 0)
                        return destIndex;

                    ctx.Read(dest, destIndex, count);
                    destIndex += count;
                }
                else
                {
                    int count3 = i & 0x3F;
                    if (count3 == 0x3E)
                    {
                        // case 4
                        int count = ctx.ReadInt16();
                        byte color = ctx.ReadByte();

                        for (int end = destIndex + count; destIndex < end; destIndex++)
                            dest[destIndex] = color;
                    }
                    else if (count3 == 0x3F)
                    {
                        // case 5
                        int count = ctx.ReadInt16();
                        int srcIndex = ctx.ReadInt16();
                        if (srcIndex >= destIndex)
                            throw new NotImplementedException(string.Format("srcIndex >= destIndex  {0}  {1}", srcIndex, destIndex));

                        for (int end = destIndex + count; destIndex < end; destIndex++)
                            dest[destIndex] = dest[srcIndex++];
                    }
                    else
                    {
                        // case 3
                        int count = count3 + 3;
                        int srcIndex = ctx.ReadInt16();
                        if (srcIndex >= destIndex)
                            throw new NotImplementedException(string.Format("srcIndex >= destIndex  {0}  {1}", srcIndex, destIndex));

                        for (int end = destIndex + count; destIndex < end; destIndex++)
                            dest[destIndex] = dest[srcIndex++];
                    }
                }
            }
        }

        public unsafe static uint DecodeInto(byte* src, byte* dest)
        {
            byte* pdest = dest;
            byte* psrc = src;

            byte* copyp;
            byte* readp = src;
            byte* writep = dest;
            byte code;
            int count;

            while (true)
            {
                code = *readp++;
                if ((~code & 0x80) != 0)
                {
                    //bit 7 = 0
                    //command 0 (0cccpppp p): copy
                    count = (code >> 4) + 3;
                    copyp = writep - (((code & 0xf) << 8) + *readp++);
                    while (count-- != 0)
                        *writep++ = *copyp++;
                }
                else
                {
                    //bit 7 = 1
                    count = code & 0x3f;
                    if ((~code & 0x40) != 0)
                    {
                        //bit 6 = 0
                        if (count == 0)
                            //end of image
                            break;
                        //command 1 (10cccccc): copy
                        while (count-- != 0)
                            *writep++ = *readp++;
                    }
                    else
                    {
                        //bit 6 = 1
                        if (count < 0x3e)
                        {
                            //command 2 (11cccccc p p): copy
                            count += 3;
                            copyp = &pdest[*(ushort*)readp];

                            readp += 2;
                            while (count-- != 0)
                                *writep++ = *copyp++;
                        }
                        else if (count == 0x3e)
                        {
                            //command 3 (11111110 c c v): fill
                            count = *(ushort*)readp;
                            readp += 2;
                            code = *readp++;
                            while (count-- != 0)
                                *writep++ = code;
                        }
                        else
                        {
                            //command 4 (copy 11111111 c c p p): copy
                            count = *(ushort*)readp;
                            readp += 2;
                            copyp = &pdest[*(ushort*)readp];
                            readp += 2;
                            while (count-- != 0)
                                *writep++ = *copyp++;
                        }
                    }
                }
            }

            return (uint)(dest - pdest);
        }


        // Quick and dirty Format80 encoder version 2
        // Uses raw copy and RLE compression
        public static byte[] Encode(byte[] src)
        {
            using (var ms = new MemoryStream())
            {
                var offset = 0;
                var left = src.Length;
                var blockStart = 0;

                while (offset < left)
                {
                    var repeatCount = CountSame(src, offset, 0xFFFF);
                    if (repeatCount >= 4)
                    {
                        // Write what we haven't written up to now
                        WriteCopyBlocks(src, blockStart, offset - blockStart, ms);

                        // Command 4: Repeat byte n times
                        ms.WriteByte(0xFE);
                        // Low byte
                        ms.WriteByte((byte)(repeatCount & 0xFF));
                        // High byte
                        ms.WriteByte((byte)(repeatCount >> 8));
                        // Value to repeat
                        ms.WriteByte(src[offset]);

                        offset += repeatCount;
                        blockStart = offset;
                    }
                    else
                    {
                        offset++;
                    }
                }

                // Write what we haven't written up to now
                WriteCopyBlocks(src, blockStart, offset - blockStart, ms);

                // Write terminator
                ms.WriteByte(0x80);

                return ms.ToArray();
            }
        }

        static int CountSame(byte[] src, int offset, int maxCount)
        {
            maxCount = Math.Min(src.Length - offset, maxCount);
            if (maxCount <= 0)
                return 0;

            var first = src[offset++];
            var count = 1;

            while (count < maxCount && src[offset++] == first)
                count++;

            return count;
        }

        static void WriteCopyBlocks(byte[] src, int offset, int count, MemoryStream output)
        {
            while (count > 0)
            {
                var writeNow = Math.Min(count, 0x3F);
                output.WriteByte((byte)(0x80 | writeNow));
                output.Write(src, offset, writeNow);

                count -= writeNow;
                offset += writeNow;
            }
        }

        /*
        // Alternate Encoding Method.
        public static byte[] Encode_Alternate(byte[] src)
        {
            MemoryStream source = new MemoryStream(src, 0, src.Length, false, true);
            MemoryStream dest = new MemoryStream(src.Length);

            // String representation of the source
            byte[] sourcebytes = new byte[source.Length];
            source.Read(sourcebytes, 0, sourcebytes.Length);
            source.Position = 0;

            // Format80 data must be opened by the transfer command w/ a length of 1
            dest.WriteByte((byte)(0x80 | 1));
            dest.WriteByte((byte)source.ReadByte());

            // Encode the source
            while (source.Position < source.Length)
            {

                // Select the method that provdes the best results for the coming bytes
                int[] copypart = isCandidateForCopyCommand(source);
                int filllength = isCandidateForFillCommand(source);
                int xferlength = isCandidateForTransferCommand(source);

                int bestmethod = Math.Max(copypart[0], Math.Max(filllength, xferlength));

                // Command #4 - run-length encoding, aka: fill
                if (bestmethod == filllength)
                {
                    byte colourval = (byte)source.ReadByte();

                    dest.WriteByte(0xfe);
                    dest.Write(BitConverter.GetBytes((short)filllength), 0, 2);
                    dest.WriteByte(colourval);

                    source.Position = source.Position - 1 + filllength;
                }

				// Either small or large copy
				else if (bestmethod == copypart[0]) {

					// Command #3 - small copy
                    if (copypart[0] <= 64)
                    {
                        dest.WriteByte((byte)(0xc0 | (copypart[0] - 3)));
                        dest.Write(BitConverter.GetBytes((short)copypart[1]), 0, 2);
					}
                       
					// Command #5 - large copy
					else {
                        dest.WriteByte(0xff);
                        dest.Write(BitConverter.GetBytes((short)copypart[0]), 0, 2);
                        dest.Write(BitConverter.GetBytes((short)copypart[1]), 0, 2);
					}

					source.Position = source.Position + copypart[0];
				}


                // Command #2 - straight transfer of bytes from source to dest
                else
                {
                    byte[] xferbytes = new byte[xferlength];
                    source.Read(xferbytes, 0, xferbytes.Length);

                    dest.WriteByte((byte)(0x80 | xferlength));
                    dest.Write(xferbytes, 0, xferbytes.Length);
                }
            }

            // SHP data must be closed by the transfer command w/ a length of 0
            dest.WriteByte(0x80);

            source.Position = 0;
            dest.SetLength(dest.Position);
            dest.Position = 0;
            return dest.ToArray();
        }

        private static int[] isCandidateForCopyCommand(MemoryStream source)
        {

            // Retain current position
            long pos = source.Position;

            // Copy of the bytes read thus far
            MemoryStream sourcecopy = new MemoryStream(source.GetBuffer());
            sourcecopy.Position = 0;

            int candidatelength = 0;
            int candidateposition = -1;

            // Search for instances of the remaining bytes in the source so far
            while ((source.Position < source.Length) && (sourcecopy.Position < sourcecopy.Length))
            {
                long copypos = sourcecopy.Position;

                // Potential match
                int runlength = 0;
                while ((source.Position < source.Length) && (sourcecopy.Position < sourcecopy.Length))
                {
                    if (source.ReadByte() == sourcecopy.ReadByte())
                    {
                        runlength++;
                    }
                    else
                    {
                        break;
                    }
                }

                // Update candidate length and position?
                if (runlength > candidatelength)
                {
                    candidatelength = runlength;
                    candidateposition = (int)copypos;
                }

                source.Position = pos;
            }

            // Reset prior position
            source.Position = pos;

            // Evaluate copy command candidacy
            return new int[] { candidatelength > 3 ? candidatelength : 0, candidateposition };
        }

        private static int isCandidateForFillCommand(MemoryStream source)
        {

            // Retain current position
            long pos = source.Position;

            // Find out how many bytes ahead have the same value as the starting byte
            int candidatelength = 1;
            byte fillbyte = (byte)source.ReadByte();

            while ((source.Position < source.Length) && candidatelength < 65535)
            {
                if (fillbyte != (byte)source.ReadByte())
                {
                    break;
                }
                candidatelength++;
            }

            // Reset prior position
            source.Position = pos;

            // Evaluate fill command candidacy
            return candidatelength > 3 ? candidatelength : 0;
        }

        private static int isCandidateForTransferCommand(MemoryStream source)
        {

            // Retain current position
            long pos = source.Position;

            // Find out the longest stretch of dissimilar bytes
            int candidatelength = 1;
            int runlength = 1;
            byte lastbyte = (byte)source.ReadByte();

            while ((source.Position < source.Length) && candidatelength < 63)
            {
                byte nextbyte = (byte)source.ReadByte();
                if (nextbyte == lastbyte)
                {
                    runlength++;
                    if (runlength > 3)
                    {
                        candidatelength -= runlength - 2;
                        break;
                    }
                }
                else
                {
                    runlength = 1;
                }
                candidatelength++;
                lastbyte = nextbyte;
            }

            // Reset prior position
            source.Position = pos;

            // Transfer command candidacy is always valid
            return candidatelength;
        }
*/

    }
}
