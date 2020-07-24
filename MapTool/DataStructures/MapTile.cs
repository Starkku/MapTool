/*
 * Copyright 2017-2020 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

namespace MapTool.DataStructures
{
    /// <summary>
    /// Class for map tiles.
    /// </summary>
    public class MapTile
    {
        /// <summary>
        /// Map tile X coordinate.
        /// </summary>
        public short X
        {
            get;
            set;
        }

        /// <summary>
        /// Map tile Y coordinate.
        /// </summary>
        public short Y
        {
            get;
            set;
        }

        /// <summary>
        /// Map tile index.
        /// </summary>
        public int TileIndex 
        {
            get;
            set;
        }

        /// <summary>
        /// Map tile sub-index.
        /// </summary>
        public byte SubTileIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Map tile height.
        /// </summary>
        public byte Level
        {
            get;
            set;
        }

        /// <summary>
        /// Map tile ice growth value.
        /// </summary>
        public byte IceGrowth
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new map tile.
        /// </summary>
        /// <param name="x">Map tile X coordinate.</param>
        /// <param name="y">Map tile Y coordinate.</param>
        /// <param name="tileIndex">Map tile index.</param>
        /// <param name="subTileIndex">Map tile sub-index.</param>
        /// <param name="level">Map tile height.</param>
        /// <param name="iceGrowth">Map tile ice growth value.</param>
        public MapTile(short x = 0, short y = 0, int tileIndex = 0, byte subTileIndex = 0, byte level = 0, byte iceGrowth = 0) 
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
