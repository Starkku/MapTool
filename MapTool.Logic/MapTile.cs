/*
 * Copyright 2017-2020 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 2 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

using System.ComponentModel;

namespace MapTool.Logic
{
    /// <summary>
    /// Class for map tiles.
    /// </summary>
    public class MapTile : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private short _x;

        /// <summary>
        /// Map tile X coordinate.
        /// </summary>
        public short X
        {
            get { return _x; }
            set
            {
                _x = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(X)));
            }
        }

        private short _y;

        /// <summary>
        /// Map tile Y coordinate.
        /// </summary>
        public short Y
        {
            get { return _y; }
            set
            {
                _y = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y)));
            }
        }

        private int _tileIndex;

        /// <summary>
        /// Map tile index.
        /// </summary>
        public int TileIndex
        {
            get { return _tileIndex; }
            set
            {
                _tileIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TileIndex)));
            }
        }

        private byte _subTileIndex;

        /// <summary>
        /// Map tile sub-index.
        /// </summary>
        public byte SubTileIndex
        {
            get { return _subTileIndex; }
            set
            {
                _subTileIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubTileIndex)));
            }
        }

        private byte _level;

        /// <summary>
        /// Map tile height.
        /// </summary>
        public byte Level
        {
            get { return _level; }
            set
            {
                _level = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Level)));
            }
        }

        private byte _iceGrowth;

        /// <summary>
        /// Map tile ice growth value.
        /// </summary>
        public byte IceGrowth
        {
            get { return _iceGrowth; }
            set
            {
                _iceGrowth = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IceGrowth)));
            }
        }

        /// <summary>
        /// Gets whether or not this map tile is a valid map tile.
        /// </summary>
        public bool IsValid => TileIndex >= 0 && TileIndex <= ushort.MaxValue
            && SubTileIndex >= 0 && SubTileIndex <= byte.MaxValue && Level >= 0 && Level <= 14;

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
            TileIndex = tileIndex == ushort.MaxValue ? 0 : tileIndex;
            SubTileIndex = subTileIndex;
            Level = level;
            IceGrowth = iceGrowth;
        }


    }
}
