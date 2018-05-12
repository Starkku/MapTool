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
    public class ByteIDConversionRule
    {

        public int OriginalStartIndex
        {
            get;
            private set;
        }
        public int OriginalEndIndex
        {
            get;
            private set;
        }
        public int NewStartIndex
        {
            get;
            private set;
        }
        public int NewEndIndex
        {
            get;
            private set;
        }
        public int HeightOverride
        {
            get;
            private set;
        }
        public int SubIndexOverride
        {
            get;
            private set;
        }

        public ByteIDConversionRule(int originalStartIndex, int newStartIndex, int originalEndIndex = -1, int newEndIndex = -1, int heightOverride = -1, int subIndexOverride = -1)
        {
            OriginalStartIndex = originalStartIndex;
            if (originalEndIndex < 0) OriginalEndIndex = originalStartIndex;
            else OriginalEndIndex = originalEndIndex;
            NewStartIndex = newStartIndex;
            if (newEndIndex < 0) NewEndIndex = newStartIndex;
            else NewEndIndex = newEndIndex;
            HeightOverride = heightOverride;
            SubIndexOverride = subIndexOverride;
        }

        public bool ValidForOverlays()
        {
            if (OriginalStartIndex > 254) return false;
            else if (OriginalEndIndex > 254) return false;
            else if (NewStartIndex > 255) return false;
            else if (NewEndIndex > 255) return false;
            return true;
        }
    }

    public class StringIDConversionRule
    {

        public string Original
        {
            get;
            private set;
        }
        public string New
        {
            get;
            private set;
        }

        public StringIDConversionRule(string original, string replacement)
        {
            Original = original;
            New = replacement;
        }
    }

    public class SectionConversionRule
    {
        public string OriginalSection
        {
            get;
            private set;
        }
        public string NewSection
        {
            get;
            private set;
        }
        public string OriginalKey
        {
            get;
            private set;
        }
        public string NewKey
        {
            get;
            private set;
        }
        public string NewValue
        {
            get;
            private set;
        }
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
