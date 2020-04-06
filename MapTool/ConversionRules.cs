/*
 * Copyright 2017 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

namespace MapTool
{
    public enum SectionRuleType { Replace, Add, Remove };

    public abstract class ByteIDConversionRule
    {
        public int OriginalStartIndex { get; private set; } = -1;

        public int OriginalEndIndex { get; private set; } = -1;

        public int NewStartIndex { get; private set; } = -1;

        public int NewEndIndex { get; private set; } = -1;

        public int CoordinateFilterX { get; private set; } = -1;

        public int CoordinateFilterY { get; private set; } = -1;

        public bool IsRandomizer { get; private set; }

        public abstract bool IsValid { get; }

        public ByteIDConversionRule(int originalStartIndex, int newStartIndex, int originalEndIndex, int newEndIndex, 
            bool isRandomizer, int coordinateFilterX, int coordinateFilterY)
        {
            OriginalStartIndex = originalStartIndex;
            if (originalEndIndex < 0)
                OriginalEndIndex = originalStartIndex;
            else
                OriginalEndIndex = originalEndIndex;

            NewStartIndex = newStartIndex;
            if (newEndIndex < 0)
                NewEndIndex = newStartIndex;
            else
                NewEndIndex = newEndIndex;

            IsRandomizer = isRandomizer;
            CoordinateFilterX = coordinateFilterX;
            CoordinateFilterY = coordinateFilterY;
        }

        public ByteIDConversionRule()
        {
        }
    }

    public class TileConversionRule : ByteIDConversionRule
    {
        public int HeightOverride { get; private set; } = -1;

        public int SubIndexOverride { get; private set; } = -1;

        public int IceGrowthOverride { get; private set; } = -1;

        public override bool IsValid { get { return CheckValidity(); } }

        public TileConversionRule(int originalStartIndex, int newStartIndex, int originalEndIndex = -1, int newEndIndex = -1, 
            bool isRandomizer = false, int heightOverride = -1, int subIndexOverride = -1, int iceGrowthOverride = -1, int coordinateFilterX = -1, int coordinateFilterY = -1) :
            base(originalStartIndex, newStartIndex, originalEndIndex, newEndIndex, isRandomizer, coordinateFilterX, coordinateFilterY)
        {
            HeightOverride = heightOverride;
            SubIndexOverride = subIndexOverride;
            IceGrowthOverride = iceGrowthOverride;
        }

        public TileConversionRule() : base()
        {
        }

        private bool CheckValidity()
        {
            if (OriginalStartIndex > 65535 || OriginalEndIndex > 65535 || NewStartIndex > 65535 || NewEndIndex > 65535)
                return false;
            return true;
        }
    }

    public class OverlayConversionRule : ByteIDConversionRule
    {

        public int OriginalStartFrameIndex { get; private set; } = -1;

        public int OriginalEndFrameIndex { get; private set; } = -1;

        public int NewStartFrameIndex { get; private set; } = -1;

        public int NewEndFrameIndex { get; private set; } = -1;

        public bool IsFrameRandomizer { get; private set; }

        public override bool IsValid { get { return CheckValidity(); } }

        public OverlayConversionRule(int originalStartIndex, int newStartIndex, int originalEndIndex = -1, int newEndIndex = -1, bool isRandomizer = false,
            int originalStartFrameIndex = -1, int newStartFrameIndex = -1, int originalEndFrameIndex = -1, int newEndFrameIndex = -1, bool isFrameRandomizer = false,
            int coordinateFilterX = -1, int coordinateFilterY = -1) :
            base(originalStartIndex, newStartIndex, originalEndIndex, newEndIndex, isRandomizer, coordinateFilterX, coordinateFilterY)
        {
            OriginalStartFrameIndex = originalStartFrameIndex;
            if (originalEndFrameIndex < 0)
                OriginalEndFrameIndex = originalStartFrameIndex;
            else
                OriginalEndFrameIndex = originalEndFrameIndex;

            NewStartFrameIndex = newStartFrameIndex;
            if (newEndFrameIndex < 0)
                NewEndFrameIndex = newStartFrameIndex;
            else
                NewEndFrameIndex = newEndFrameIndex;

            IsFrameRandomizer = isFrameRandomizer;
        }

        public OverlayConversionRule() : base()
        {
        }

        private bool CheckValidity()
        {
            if (((OriginalStartIndex > 254 || OriginalEndIndex > 254) && CoordinateFilterX < 0 && CoordinateFilterY < 0) || NewStartIndex > 255 || NewEndIndex > 255 ||
                NewStartFrameIndex > 255 || NewEndFrameIndex > 255)
                return false;

            return true;
        }
    }

    public class StringIDConversionRule
    {

        public string Original { get; private set; }
        public string New { get; private set; }

        public StringIDConversionRule(string original, string replacement)
        {
            Original = original;
            New = replacement;
        }
    }

    public class SectionConversionRule
    {
        public string OriginalSection { get; private set; }
        public string NewSection { get; private set; }
        public string OriginalKey { get; private set; }
        public string NewKey { get; private set; }
        public string NewValue { get; private set; }

        public SectionConversionRule(string originalSection, string newSection, string originalKey, string newKey, string newValue)
        {
            OriginalSection = originalSection;
            NewSection = newSection;
            OriginalKey = originalKey;
            NewKey = newKey;
            NewValue = newValue;
        }
    }
}
