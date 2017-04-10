using System;
using System.Collections.Generic;
using System.Text;

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
        public string List
        {
            get;
            set;
        }
        public bool Convert
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
