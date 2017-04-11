/*
 * Copyright 2017 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

namespace MapTool.Utility
{
    struct Settings
    {
        public bool ShowHelp
        {
            get;
            set;
        }
        public string FileInput
        {
            get;
            set;
        }
        public string FileOutput
        {
            get;
            set;
        }
        public string FileConfig
        {
            get;
            set;
        }
        public bool List
        {
            get;
            set;
        }
        public bool DebugLogging
        {
            get;
            set;
        }
    }
}
