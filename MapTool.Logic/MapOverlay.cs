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
    /// Class for map overlays.
    /// </summary>
    public class MapOverlay : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private short _x;

        /// <summary>
        /// Map overlay X coordinate.
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
        /// Map overlay Y coordinate.
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

        private byte _index;

        /// <summary>
        /// Map overlay type index.
        /// </summary>
        public byte Index
        {
            get { return _index; }
            set
            {
                _index = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Index)));
            }
        }

        private byte _frameIndex;

        /// <summary>
        /// Map overlay frame index.
        /// </summary>
        public byte FrameIndex
        {
            get { return _frameIndex; }
            set
            {
                _frameIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FrameIndex)));
            }
        }

        /// <summary>
        /// Creates a new map overlay.
        /// </summary>
        /// <param name="x">Map overlay X coordinate.</param>
        /// <param name="y">Map overlay Y coordinate.</param>
        /// <param name="index">Map overlay type index.</param>
        /// <param name="frameIndex">Map overlay frame index.</param>
        public MapOverlay(short x, short y, byte index, byte frameIndex)
        {
            X = x;
            Y = y;
            Index = index;
            FrameIndex = frameIndex;
        }
    }
}
