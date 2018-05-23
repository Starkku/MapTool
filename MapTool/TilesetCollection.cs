/*
 * Copyright 2017 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

using System.Collections;
using StarkkuUtils.FileTypes;
using StarkkuUtils.Utilities;

namespace MapTool
{
    class TilesetCollection : CollectionBase
    {

        public TilesetCollection()
        {
        }

        #region Properties
        /// <summary>
        /// Gets/Sets value for the item by that index
        /// </summary>
        public Tileset this[int index]
        {
            get
            {
                return (Tileset)List[index];
            }
            set
            {
                List[index] = value;
            }
        }
        #endregion

        #region Public Methods

        public int IndexOf(Tileset tilesetItem)
        {
            if (tilesetItem != null)
            {
                return base.List.IndexOf(tilesetItem);
            }
            return -1;
        }

        public int Add(Tileset tilesetItem)
        {
            if (tilesetItem != null)
            {
                return List.Add(tilesetItem);
            }
            return -1;
        }

        public void Remove(Tileset tilesetItem)
        {
            InnerList.Remove(tilesetItem);
        }

        public void AddRange(TilesetCollection collection)
        {
            if (collection != null)
            {
                InnerList.AddRange(collection);
            }
        }

        public void Insert(int index, Tileset tilesetItem)
        {
            if (index <= List.Count && tilesetItem != null)
            {
                List.Insert(index, tilesetItem);
            }
        }

        public bool Contains(Tileset tilesetItem)
        {
            return List.Contains(tilesetItem);
        }

        #endregion

        public static TilesetCollection ParseFromINIFile(INIFile theaterConfig)
        {
            TilesetCollection tsc = new TilesetCollection();
            Tileset ts;
            string[] sections = theaterConfig.GetSections();
            foreach (string section in sections)
            {
                if (!section.StartsWith("TileSet")) continue;
                ts = new Tileset
                {
                    SetID = section,
                    SetNumber = Conversion.GetIntFromString(section.Substring(7, 4), -1),
                    SetName = theaterConfig.GetKey(section, "SetName", "N/A"),
                    FileName = theaterConfig.GetKey(section, "FileName", "N/A").ToLower(),
                    TilesInSet = Conversion.GetIntFromString(theaterConfig.GetKey(section, "TilesInSet", "0"), 0)
                };
                if (ts.SetNumber == -1)
                    continue;
                tsc.Add(ts);
            }
            return tsc;
        }
    }

    public class Tileset
    {
        public string SetID { get; set; }
        public int SetNumber { get; set; }
        public string SetName { get; set; }
        public string FileName { get; set; }
        public int TilesInSet { get; set; }

        public Tileset()
        {
        }

        public Tileset(string setid, int setnumber, string setname, string filename, int tilesinset)
        {
            SetID = setid;
            SetNumber = setnumber;
            SetName = setname;
            FileName = filename;
            TilesInSet = tilesinset;
        }

        public string[] GetPrintableData(int tileCounter)
        {
            string[] data = new string[4];
            data[0] = SetID + " | " + SetName;
            data[1] = "Filename: " + FileName;
            data[2] = "Number of tiles: " + TilesInSet;
            data[3] = "Range: " + tileCounter.ToString() + "-" + ((tileCounter + TilesInSet) - 1).ToString();
            return data;
        }
    }
}
