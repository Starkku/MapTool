/*
 * Copyright 2017-2020 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 2 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace MapTool.Logic
{
    internal abstract class IndexConversionRule
    {
        /// <summary>
        /// Original start index number.
        /// </summary>
        public int OriginalStartIndex { get; private set; } = -1;

        /// <summary>
        /// Original end index number.
        /// </summary>
        public int OriginalEndIndex { get; private set; } = -1;

        /// <summary>
        /// New start index number.
        /// </summary>
        public int NewStartIndex { get; private set; } = -1;

        /// <summary>
        /// New end index number.
        /// </summary>
        public int NewEndIndex { get; private set; } = -1;

        /// <summary>
        /// Map tile coordinate filter for X coordinate.
        /// </summary>
        public int CoordinateFilterX { get; private set; } = -1;

        /// <summary>
        /// Map tile coordinate filter for Y coordinate.
        /// </summary>
        public int CoordinateFilterY { get; private set; } = -1;

        /// <summary>
        /// Is new index picked randomly from range defined by start and end index?
        /// </summary>
        public bool IsRandomizer { get; private set; }

        public abstract bool IsValid { get; }

        public IndexConversionRule(int originalStartIndex, int newStartIndex, int originalEndIndex, int newEndIndex,
            bool isRandomizer, int coordinateFilterX, int coordinateFilterY)
        {
            OriginalStartIndex = originalStartIndex;
            if (originalEndIndex < 0)
                OriginalEndIndex = originalStartIndex;
            else
                OriginalEndIndex = originalEndIndex;

            NewStartIndex = newStartIndex;
            if (newEndIndex < 0)
                NewEndIndex = newStartIndex;
            else
                NewEndIndex = newEndIndex;

            IsRandomizer = isRandomizer;
            CoordinateFilterX = coordinateFilterX;
            CoordinateFilterY = coordinateFilterY;
        }

        public IndexConversionRule()
        {
        }
    }

    /// <summary>
    /// Terrain tile conversion rule.
    /// </summary>
    internal class TileConversionRule : IndexConversionRule
    {
        /// <summary>
        /// Tile height override.
        /// </summary>
        public int HeightOverride { get; private set; } = -1;

        /// <summary>
        /// Tile sub-index override.
        /// </summary>
        public int SubIndexOverride { get; private set; } = -1;

        /// <summary>
        /// Tile ice growth value override.
        /// </summary>
        public int IceGrowthOverride { get; private set; } = -1;

        /// <summary>
        /// Original sub-tile start index number.
        /// </summary>
        public int OriginalSubStartIndex { get; private set; } = -1;

        /// <summary>
        /// Original sub-tile end index number.
        /// </summary>
        public int OriginalSubEndIndex { get; private set; } = -1;

        /// <summary>
        /// New sub-tile start index number.
        /// </summary>
        public int NewSubStartIndex { get; private set; } = -1;

        /// <summary>
        /// New sub-tile end index number.
        /// </summary>
        public int NewSubEndIndex { get; private set; } = -1;

        /// <summary>
        /// Is new sub-tile index picked randomly from range defined by start and end index?
        /// </summary>
        public bool IsSubRandomizer { get; private set; }

        /// <summary>
        /// Whether or not the rule is valid.
        /// </summary>
        public override bool IsValid { get { return CheckValidity(); } }

        /// <summary>
        /// Creates a new terrain tile conversion rule.
        /// </summary>
        /// <param name="originalStartIndex">Original tile start index.</param>
        /// <param name="newStartIndex">New tile start index.</param>
        /// <param name="originalEndIndex">Original tile end index.</param>
        /// <param name="newEndIndex">New tile end index.</param>
        /// <param name="isRandomizer">Set to true for new index to be randomly picked from range defined by start and end index.</param>
        /// <param name="heightOverride">Tile height override.</param>
        /// <param name="subIndexOverride">Tile sub-index override.</param>
        /// <param name="iceGrowthOverride">Tile ice growth value override.</param>
        /// <param name="coordinateFilterX">Map tile coordinate filter for X coordinate.</param>
        /// <param name="coordinateFilterY">Map tile coordinate filter for Y coordinate.</param>
        /// <param name="originalSubStartIndex">Original sub-tile start index.</param>
        /// <param name="newSubStartIndex">New sub-tile start index.</param>
        /// <param name="originalSubEndIndex">Original sub-tile end index.</param>
        /// <param name="newSubEndIndex">New sub-tile end index.</param>
        public TileConversionRule(int originalStartIndex, int newStartIndex, int originalEndIndex = -1, int newEndIndex = -1,
            bool isRandomizer = false, int heightOverride = -1, int subIndexOverride = -1, int iceGrowthOverride = -1, int coordinateFilterX = -1, int coordinateFilterY = -1,
            int originalSubStartIndex = -1, int newSubStartIndex = -1, int originalSubEndIndex = -1, int newSubEndIndex = -1, bool isSubRandomizer = false) :
            base(originalStartIndex, newStartIndex, originalEndIndex, newEndIndex, isRandomizer, coordinateFilterX, coordinateFilterY)
        {
            HeightOverride = heightOverride;
            SubIndexOverride = subIndexOverride;
            IceGrowthOverride = iceGrowthOverride;
            OriginalSubStartIndex = originalSubStartIndex;
            NewSubStartIndex = newSubStartIndex;
            OriginalSubEndIndex = originalSubEndIndex;
            NewSubEndIndex = newSubEndIndex;
            IsSubRandomizer = isSubRandomizer;
        }

        /// <summary>
        /// Creates a new terrain tile conversion rule with default values.
        /// </summary>
        public TileConversionRule() : base()
        {
        }

        private bool CheckValidity()
        {
            if (OriginalStartIndex > 65535 || OriginalEndIndex > 65535 || NewStartIndex > 65535 || NewEndIndex > 65535)
                return false;
            return true;
        }
    }

    /// <summary>
    /// Overlay object conversion rule.
    /// </summary>
    internal class OverlayConversionRule : IndexConversionRule
    {
        /// <summary>
        /// Original overlay frame start index.
        /// </summary>
        public int OriginalStartFrameIndex { get; private set; } = -1;

        /// <summary>
        /// Original overlay frame end index.
        /// </summary>
        public int OriginalEndFrameIndex { get; private set; } = -1;

        /// <summary>
        /// New overlay frame start index.
        /// </summary>
        public int NewStartFrameIndex { get; private set; } = -1;

        /// <summary>
        /// New overlay frame end index.
        /// </summary>
        public int NewEndFrameIndex { get; private set; } = -1;

        /// <summary>
        /// Is new frame index picked randomly from range defined by start and end index?
        /// </summary>
        public bool IsFrameRandomizer { get; private set; }

        /// <summary>
        /// Whether or not the rule is valid.
        /// </summary>
        public override bool IsValid { get { return CheckValidity(); } }

        /// <summary>
        /// Creates a new overlay object conversion rule.
        /// </summary>
        /// <param name="originalStartIndex">Original overlay start index.</param>
        /// <param name="newStartIndex">New overlay start index.</param>
        /// <param name="originalEndIndex">Original overlay end index.</param>
        /// <param name="newEndIndex">New overlay end index.</param>
        /// <param name="isRandomizer">Set to true for new index to be randomly picked from range defined by start and end index.</param>
        /// <param name="originalStartFrameIndex">Original overlay frame index.</param>
        /// <param name="newStartFrameIndex">New overlay frame index.</param>
        /// <param name="originalEndFrameIndex">Original overlay frame end index.</param>
        /// <param name="newEndFrameIndex">New overlay frame end index.</param>
        /// <param name="isFrameRandomizer">Set to true for new frame index to be randomly picked from range defined by start and end index.</param>
        /// <param name="coordinateFilterX">Map tile coordinate filter for X coordinate.</param>
        /// <param name="coordinateFilterY">Map tile coordinate filter for Y coordinate.</param>
        public OverlayConversionRule(int originalStartIndex, int newStartIndex, int originalEndIndex = -1, int newEndIndex = -1, bool isRandomizer = false,
            int originalStartFrameIndex = -1, int newStartFrameIndex = -1, int originalEndFrameIndex = -1, int newEndFrameIndex = -1, bool isFrameRandomizer = false,
            int coordinateFilterX = -1, int coordinateFilterY = -1) :
            base(originalStartIndex, newStartIndex, originalEndIndex, newEndIndex, isRandomizer, coordinateFilterX, coordinateFilterY)
        {
            OriginalStartFrameIndex = originalStartFrameIndex;
            if (originalEndFrameIndex < 0)
                OriginalEndFrameIndex = originalStartFrameIndex;
            else
                OriginalEndFrameIndex = originalEndFrameIndex;

            NewStartFrameIndex = newStartFrameIndex;
            if (newEndFrameIndex < 0)
                NewEndFrameIndex = newStartFrameIndex;
            else
                NewEndFrameIndex = newEndFrameIndex;

            IsFrameRandomizer = isFrameRandomizer;
        }

        /// <summary>
        /// Creates a new overlay object conversion rule with default values.
        /// </summary>
        public OverlayConversionRule() : base()
        {
        }

        private bool CheckValidity()
        {
            if (((OriginalStartIndex > 254 || OriginalEndIndex > 254) && CoordinateFilterX < 0 && CoordinateFilterY < 0) || NewStartIndex > 255 || NewEndIndex > 255 ||
                NewStartFrameIndex > 255 || NewEndFrameIndex > 255)
                return false;

            return true;
        }
    }

    /// <summary>
    /// General object conversion rule.
    /// </summary>
    internal class ObjectConversionRule
    {
        /// <summary>
        /// Original object name.
        /// </summary>
        public string OriginalName { get; private set; }

        /// <summary>
        /// New object name.
        /// </summary>
        public string NewName { get; private set; }

        /// <summary>
        /// Map tile coordinate filter for X coordinate.
        /// </summary>
        public int CoordinateFilterX { get; private set; } = -1;

        /// <summary>
        /// Map tile coordinate filter for Y coordinate.
        /// </summary>
        public int CoordinateFilterY { get; private set; } = -1;

        /// <summary>
        /// Original object upgrade names.
        /// </summary>
        public string[] OriginalUpgrades { get; private set; } = new string[3] { "*", "*", "*" };

        /// <summary>
        /// New object upgrade names.
        /// </summary>
        public string[] NewUpgrades { get; private set; } = new string[3] { "*", "*", "*" };

        /// <summary>
        /// Creates new general object conversion rule.
        /// </summary>
        /// <param name="originalName">Original object name.</param>
        /// <param name="newName">New object name.<param>
        /// <param name="originalUpgrades">Old object upgrade names.</param>/>
        /// <param name="newUpgrades">New object upgrade names.</param>/>
        /// <param name="coordinateFilterX">Map tile coordinate filter for X coordinate.</param>
        /// <param name="coordinateFilterY">Map tile coordinate filter for Y coordinate.</param>
        public ObjectConversionRule(string originalName, string newName, IList<string> originalUpgrades, IList<string> newUpgrades, int coordinateFilterX = -1, int coordinateFilterY = -1)
        {
            OriginalName = originalName;
            NewName = newName;

            for (int i = 0; i < Math.Min(OriginalUpgrades.Length, originalUpgrades.Count); i++)
            {
                OriginalUpgrades[i] = originalUpgrades[i];
            }

            for (int i = 0; i < Math.Min(NewUpgrades.Length, newUpgrades.Count); i++)
            {
                NewUpgrades[i] = newUpgrades[i];
            }

            CoordinateFilterX = coordinateFilterX;
            CoordinateFilterY = coordinateFilterY;
        }
    }

    /// <summary>
    /// INI section conversion rule.
    /// </summary>
    internal class SectionConversionRule
    {
        /// <summary>
        /// Original section name.
        /// </summary>
        public string OriginalSection { get; private set; }
        /// <summary>
        /// New section name.
        /// </summary>
        public string NewSection { get; private set; }
        /// <summary>
        /// Original key.
        /// </summary>
        public string OriginalKey { get; private set; }
        /// <summary>
        /// New key.
        /// </summary>
        public string NewKey { get; private set; }
        /// <summary>
        /// New value.
        /// </summary>
        public string NewValue { get; private set; }

        /// <summary>
        /// Creates a new INI section conversion rule.
        /// </summary>
        /// <param name="originalSection">Original section name.</param>
        /// <param name="newSection">New section name.</param>
        /// <param name="originalKey">Original key.</param>
        /// <param name="newKey">New key.</param>
        /// <param name="newValue">New value.</param>
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
