using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapTool
{

    class MapTileContainer
    {

        public short X
        {
            get;
            set;
        }
        public short Y
        {
            get;
            set;
        }
        public int TileIndex 
        {
            get;
            set;
        }
        public byte SubTileIndex
        {
            get;
            set;
        }

        public byte Level
        {
            get;
            set;
        }
        public byte UData
        {
            get;
            set;
        }

        public MapTileContainer(short x = 0, short y = 0, int tileindex = 0, byte subtileindex = 0, byte level = 0, byte udata2 = 0) 
        {
            X = x;
            Y = y;
            TileIndex = tileindex;
            SubTileIndex = subtileindex;
            Level = level;
            UData = udata2;
        }
    }
}
