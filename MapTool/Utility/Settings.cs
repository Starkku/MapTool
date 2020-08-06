/*
 * Copyright 2017-2020 by Starkku
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
        /// <summary>
        /// Show help on usage on startup.
        /// </summary>
        public bool ShowHelp
        {
            get;
            set;
        }

        /// <summary>
        /// Input filename.
        /// </summary>
        public string FileInput
        {
            get;
            set;
        }

        /// <summary>
        /// Output filename.
        /// </summary>
        public string FileOutput
        {
            get;
            set;
        }

        /// <summary>
        /// Conversion profile filename.
        /// </summary>
        public string FileConfig
        {
            get;
            set;
        }

        /// <summary>
        /// If set, output tileset data based on input theater config INI file.
        /// </summary>
        public bool List
        {
            get;
            set;
        }

        /// <summary>
        /// If set, writes a log file.
        /// </summary>
        public bool WriteLogFile
        {
            get;
            set;
        }

        /// <summary>
        /// If set, shows debug-level logging in console.
        /// </summary>
        public bool ShowDebugLogging
        {
            get;
            set;
        }
    }
}
