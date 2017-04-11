/*
 * Copyright 2017 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

using System;
using System.Collections;
using MapTool.Utility;

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

        public static TilesetCollection ParseFromINIFile(INIFile theaterconfig)
        {
            TilesetCollection tsc = new TilesetCollection();
            Tileset ts;
            int tmp;
            string[] sections = theaterconfig.GetSections();
            foreach (string section in sections)
            {
                if (!section.StartsWith("TileSet")) continue;
                ts = new Tileset();
                ts.SetID = section;
                Int32.TryParse(section.Substring(7, 4), out tmp);
                ts.SetNumber = tmp;
                ts.SetName = theaterconfig.GetKey(section, "SetName", null);
                ts.FileName = theaterconfig.GetKey(section, "FileName", null);
                if (ts.FileName != null) ts.FileName = ts.FileName.ToLower();
                try
                {
                    ts.TilesInSet = GetInt(theaterconfig.GetKey(section, "TilesInSet", "N/A"));
                }
                catch (Exception)
                {
                    ts.TilesInSet = 0;
                }
                tsc.Add(ts);
            }
            return tsc;
        }

        private static int GetInt(string str)
        {
            int i = -1;
            try
            {
                i = Int32.Parse(str);
            }
            catch (Exception)
            {
            }
            return i;
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

        public string[] getPrintableData(ref int tilecounter)
        {
            string[] data = new string[4];
            data[0] = SetID + " | " + SetName;
            data[1] = "Filename: " + FileName;
            data[2] = "Number of tiles: " + TilesInSet;
            data[3] = "Range: " + tilecounter.ToString() + "-" + ((tilecounter += TilesInSet) - 1).ToString();
            return data;
        }
    }
}
