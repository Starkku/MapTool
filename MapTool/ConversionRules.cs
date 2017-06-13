/*
 * Copyright 2017 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

using System.Collections.Generic;

namespace MapTool
{
    public enum SectionRuleType { Replace, Add, Remove };
    public class ByteIDConversionRule
    {

        public int Original_Start
        {
            get;
            private set;
        }
        public int Original_End
        {
            get;
            private set;
        }
        public int New_Start
        {
            get;
            private set;
        }
        public int New_End
        {
            get;
            private set;
        }
        public int HeightOverride
        {
            get;
            private set;
        }
        public int SubIdxOverride
        {
            get;
            private set;
        }

        public ByteIDConversionRule(int original_start, int new_start, int original_end = -1, int new_end = -1, int heightovr = -1, int subtovr = -1)
        {
            Original_Start = original_start;
            if (original_end < 0) Original_End = original_start;
            else Original_End = original_end;
            New_Start = new_start;
            if (new_end < 0) New_End = new_start;
            else New_End = new_end;
            HeightOverride = heightovr;
            SubIdxOverride = subtovr;
        }

        public bool ValidForOverlays()
        {
            if (Original_Start > 254) return false;
            else if (Original_End > 254) return false;
            else if (New_Start > 255) return false;
            else if (New_End > 255) return false;
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
        public string Original_Section
        {
            get;
            private set;
        }
        public string New_Section
        {
            get;
            private set;
        }
        public string Original_Key
        {
            get;
            private set;
        }
        public string New_Key
        {
            get;
            private set;
        }
        public string New_Value
        {
            get;
            private set;
        }
        public SectionConversionRule(string original_section, string new_section, string original_key, string new_key, string new_value)
        {
            Original_Section = original_section;
            New_Section = new_section;
            Original_Key = original_key;
            New_Key = new_key;
            New_Value = new_value;
        }
    }
}
