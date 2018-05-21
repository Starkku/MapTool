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
        public byte IceGrowth
        {
            get;
            set;
        }

        public MapTileContainer(short x = 0, short y = 0, int tileIndex = 0, byte subTileIndex = 0, byte level = 0, byte iceGrowth = 0) 
        {
            X = x;
            Y = y;
            TileIndex = tileIndex;
            SubTileIndex = subTileIndex;
            Level = level;
            IceGrowth = iceGrowth;
        }
    }
}
