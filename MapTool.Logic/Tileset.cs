/*
 * Copyright 2017-2020 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 2 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

namespace MapTool.Logic
{
    /// <summary>
    /// Class for terrain tileset.
    /// </summary>
    internal class Tileset
    {
        /// <summary>
        /// Full tileset ID.
        /// </summary>
        public string SetID { get; set; }

        /// <summary>
        /// Number of the tileset.
        /// </summary>
        public int SetNumber { get; set; }

        /// <summary>
        /// Name of the tileset.
        /// </summary>
        public string SetName { get; set; }

        /// <summary>
        /// Filename of tiles in the tileset.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Number of tiles in the tileset.
        /// </summary>
        public int TilesInSet { get; set; }

        /// <summary>
        /// Creates a new tileset with default values.
        /// </summary>
        public Tileset()
        {
        }

        /// <summary>
        /// Creates a new tileset.
        /// </summary>
        /// <param name="setID">Full tileset ID.</param>
        /// <param name="setNumber">Number of the tileset.</param>
        /// <param name="setName">Name of the tileset.</param>
        /// <param name="filename">Filename of tiles in the tileset.</param>
        /// <param name="tilesInSet">Number of tiles in the tileset.</param>
        public Tileset(string setID, int setNumber, string setName, string filename, int tilesInSet)
        {
            SetID = setID;
            SetNumber = setNumber;
            SetName = setName;
            FileName = filename;
            TilesInSet = tilesInSet;
        }

        /// <summary>
        /// Returns tileset data in printable format.
        /// </summary>
        /// <param name="numberOfPrecedingTiles">Total number of tiles preceding this tileset.</param>
        /// <returns>Tileset data in printable format.</returns>
        public string[] GetPrintableData(int numberOfPrecedingTiles)
        {
            string[] data = new string[4];
            data[0] = SetID + " | " + SetName;
            data[1] = "Filename: " + FileName;
            data[2] = "Number of tiles: " + TilesInSet;
            data[3] = "Range: " + numberOfPrecedingTiles.ToString() + "-" + ((numberOfPrecedingTiles + TilesInSet) - 1).ToString();
            return data;
        }
    }
}
