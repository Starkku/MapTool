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
using Nini.Config;

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

        public static TilesetCollection ParseFromINIFile(IniConfigSource theaterconfig)
        {
            TilesetCollection tsc = new TilesetCollection();
            Tileset ts;
            int tmp;
            foreach (IniConfig config in theaterconfig.Configs) 
            {
                if (!config.Name.StartsWith("TileSet")) continue;
                ts = new Tileset();
                ts.SetID = config.Name;
                Int32.TryParse(config.Name.Substring(7, 4), out tmp);
                ts.SetNumber = tmp;
                ts.SetName = config.GetString("SetName", null);
                ts.FileName = config.GetString("FileName", null).ToLower();
                try
                {
                    ts.TilesInSet = config.GetInt("TilesInSet", -1);
                }
                catch (Exception)
                {
                    ts.TilesInSet = 0;
                }
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

        public string getPrintableData(ref int tilecounter)
        {
            return SetID + " | " + SetName + "\r\nFilename: " +  FileName + "\r\n" + "Number of tiles: " + TilesInSet + "\r\n" + "Range: " + tilecounter.ToString() + "-" + ((tilecounter += TilesInSet) - 1).ToString() + "\r\n\r\n";
        }
    }
}
