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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using Starkku.Utilities;
using Starkku.Utilities.FileTypes;

namespace MapTool.Logic
{
    /// <summary>
    /// Command & Conquer: Tiberian Sun / Command & Conquer: Red Alert 2 map file.
    /// </summary>
    public class MapFile : INIFile
    {
        #region public_properties

        /// <summary>
        /// Gets or sets filename for the map file.
        /// </summary>
        public new string Filename { get; set; }

        /// <summary>
        /// Gets whether or not any map tile or overlay data or INI sections, keys or values have been altered since last load or save.
        /// </summary>
        public new bool Altered => _altered || tileDataAltered || overlayDataAltered;

        /// <summary>
        /// Gets or sets map's full width.
        /// </summary>
        public int FullWidth
        {
            get
            {
                string[] split = GetKey("Map", "Size", string.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length < 4)
                    return 0;

                return Conversion.GetIntFromString(split[2], 0);
            }
            set
            {
                SetKey("Map", "Size", "0,0," + value + "," + FullHeight);
            }
        }


        /// <summary>
        /// Gets or sets map's full height.
        /// </summary>
        public int FullHeight
        {
            get
            {
                string[] split = GetKey("Map", "Size", string.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length < 4)
                    return 0;

                return Conversion.GetIntFromString(split[3], 0);
            }
            set
            {
                SetKey("Map", "Size", "0,0," + FullWidth + "," + value);
            }
        }

        /// <summary>
        /// Gets or sets map's local size left value.
        /// </summary>
        public int LocalLeft
        {
            get
            {
                string[] split = GetKey("Map", "LocalSize", string.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length < 4)
                    return 0;

                return Conversion.GetIntFromString(split[0], 0);
            }
            set
            {
                SetKey("Map", "LocalSize", value + "," + LocalTop + "," + LocalWidth + "," + LocalHeight);
            }
        }


        /// <summary>
        /// Gets or sets map's local size left value.
        /// </summary>
        public int LocalTop
        {
            get
            {
                string[] split = GetKey("Map", "LocalSize", string.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length < 4)
                    return 0;

                return Conversion.GetIntFromString(split[1], 0);
            }
            set
            {
                SetKey("Map", "LocalSize", LocalLeft + "," + value + "," + LocalWidth + "," + LocalHeight);
            }
        }

        /// <summary>
        /// Gets or sets map's local size left value.
        /// </summary>
        public int LocalWidth
        {
            get
            {
                string[] split = GetKey("Map", "LocalSize", string.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length < 4)
                    return 0;

                return Conversion.GetIntFromString(split[2], 0);
            }
            set
            {
                SetKey("Map", "LocalSize", LocalLeft + "," + LocalTop + "," + value + "," + LocalHeight);
            }
        }

        /// <summary>
        /// Gets or sets map's local size left value.
        /// </summary>
        public int LocalHeight
        {
            get
            {
                string[] split = GetKey("Map", "LocalSize", string.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length < 4)
                    return 0;

                return Conversion.GetIntFromString(split[2], 0);
            }
            set
            {
                SetKey("Map", "LocalSize", LocalLeft + "," + LocalTop + "," + LocalWidth + "," + value);
            }
        }

        /// <summary>
        /// Gets preview's width.
        /// </summary>
        public int PreviewWidth
        {
            get
            {
                string[] split = GetKey("Preview", "Size", string.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length < 4)
                    return 0;

                return Conversion.GetIntFromString(split[2], 0);
            }
            private set
            {
                SetKey("Preview", "Size", "0,0," + value + "," + PreviewHeight);
            }
        }


        /// <summary>
        /// Gets map preview's height.
        /// </summary>
        public int PreviewHeight
        {
            get
            {
                string[] split = GetKey("Preview", "Size", string.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length < 4)
                    return 0;

                return Conversion.GetIntFromString(split[3], 0);
            }
            private set
            {
                SetKey("Preview", "Size", "0,0," + PreviewWidth + "," + value);
            }
        }

        /// <summary>
        /// Gets or sets map's terrain theater.
        /// </summary>
        public string Theater
        {
            get { return GetKey("Map", "Theater", null)?.ToUpper(); }
            set
            {
                string name = value?.ToUpper();
                if (IsValidTheaterName(name))
                    SetKey("Map", "Theater", name);
            }
        }

        /// <summary>
        /// Gets whether or not the map has valid tile data loaded.
        /// </summary>
        public bool HasTileData => mapTiles != null && mapTiles.Count > 0;

        /// <summary>
        /// Gets whether or not the map has valid overlay data loaded.
        /// </summary>
        public bool HasOverlayData => mapOverlays != null && mapOverlays.Count > 0;

        #endregion

        #region private_fields

        private Dictionary<Tuple<int, int>, MapTile> mapTiles = null;
        private List<MapOverlay> mapOverlays = null;
        private Bitmap previewBitmap = null;
        private bool tileDataAltered;
        private bool overlayDataAltered;
        private bool[,] coordinateValidityLUT;

        #endregion

        /// <summary>
        /// Initializes a new map file from file.
        /// </summary>
        /// <param name="filename">Filename of map file to load.</param>
        public MapFile(string filename) : base(filename)
        {
            Initialize();
        }

        /// <summary>
        /// Initializes map data & properties.
        /// </summary>
        private void Initialize()
        {
            LoadTileData();
            LoadOverlayData();
            GetMapPreview();
        }

        /// <summary>
        /// Load the map file from specified filename, resetting all current tile, overlay, section and key data to as they are in the loaded file.
        /// </summary>
        /// <returns>Error message if something went wrong, null otherwise.</returns>
        public override string Load(string filename)
        {
            mapTiles = null;
            mapOverlays = null;
            tileDataAltered = false;
            overlayDataAltered = false;
            coordinateValidityLUT = null;

            string errorMsg = base.Load(filename);

            if (errorMsg != null)
                return errorMsg;
            else
            {
                Initialize();
                return null;
            }
        }

        /// <summary>
        /// Reload the map file using currently set filename, resetting all current section and key data to as they are in the loaded file.
        /// </summary>
        /// <returns>Error message if something went wrong, null otherwise.</returns>
        public override string Reload()
        {
            return Load(Filename);
        }

        /// <summary>
        /// Saves the map file with currently set filename.
        /// </summary>
        /// <param name="preserveWhiteSpace">If set, empty line is placed between each map file INI section.</param>
        /// <param name="saveComments">If set, comments are saved in map file.</param>
        /// <returns>Error message if something went wrong, null otherwise.</returns>
        public override string Save(bool preserveWhiteSpace = true, bool saveComments = true)
        {
            return Save(Filename, preserveWhiteSpace, saveComments);
        }

        /// <summary>
        /// Saves the map file with specified filename.
        /// </summary>
        /// <param name="filename">Filename to save the map file to.</param>
        /// <param name="preserveWhiteSpace">If set, empty line is placed between each map file INI section.</param>
        /// <param name="saveComments">If set, comments are saved in map file.</param>
        /// <returns>Error message if something went wrong, null otherwise.</returns>
        public override string Save(string filename, bool preseveWhiteSpace = true, bool saveComments = true)
        {
            SaveTileData();
            SaveOverlayData();

            string errorMsg = base.Save(filename, preseveWhiteSpace, saveComments);

            if (errorMsg != null)
                return errorMsg;
            else
            {
                tileDataAltered = false;
                overlayDataAltered = false;
                return null;
            }
        }

        /// <summary>
        /// Checks if location with given coordinates exists within map bounds.
        /// </summary>
        /// <param name="x">Location X coordinate.</param>
        /// <param name="y">Location Y coordinate.</param>
        /// <returns>True if location exists, false if not.</returns>
        public bool CoordinateExistsOnMap(int x, int y)
        {
            if (coordinateValidityLUT == null)
                CalculateCoordinateValidityLUT();

            if (coordinateValidityLUT.GetLength(0) <= x || coordinateValidityLUT.GetLength(1) <= y)
                return false;

            return coordinateValidityLUT[x, y];
        }

        /// <summary>
        /// Calculates valid map coordinates from map width & height and creates a look-up table from them.
        /// </summary>
        private void CalculateCoordinateValidityLUT()
        {
            int size = Math.Max(FullWidth, FullHeight) * 2 + 1;
            coordinateValidityLUT = new bool[size, size];

            int yOffset = 0;
            for (int col = 1; col <= FullWidth; col++)
            {
                int startY = FullWidth - yOffset;
                for (int row = 0; row < FullHeight; row++)
                {
                    int x = col + row;
                    int y = startY + row;
                    coordinateValidityLUT[x, y] = true;
                    if (col < FullWidth)
                        coordinateValidityLUT[x + 1, y] = true;
                }
                yOffset += 1;
            }
        }

        /// <summary>
        /// Checks if theater name is valid.
        /// </summary>
        /// <param name="theaterName">Theater name.</param>
        /// <returns>True if a valid theater name, otherwise false.</returns>
        private bool IsValidTheaterName(string theaterName)
        {
            if (theaterName == "TEMPERATE" || theaterName == "SNOW" || theaterName == "LUNAR" || theaterName == "DESERT" ||
                theaterName == "URBAN" || theaterName == "NEWURBAN")
                return true;

            return false;
        }

        #region map_pack_handling

        /// <summary>
        /// Loads tile data from map file if it hasn't already been loaded.
        /// </summary>
        /// <returns>Error message if something went wrong, otherwise null.</returns>
        private string LoadTileData()
        {
            if (mapTiles != null)
                return "Map tile data has already been loaded.";

            mapTiles = new Dictionary<Tuple<int, int>, MapTile>();

            int cellCount;
            byte[] isoMapPack;

            if (FullWidth < 1 || FullHeight < 1)
                return ("Map Size is invalid.");

            cellCount = (FullWidth * 2 - 1) * FullHeight;
            int lzoPackSize = cellCount * 11 + 4;
            isoMapPack = new byte[lzoPackSize];

            // Fill up and filter later
            int j = 0;

            for (int i = 0; i < cellCount; i++)
            {
                isoMapPack[j] = 0x88;
                isoMapPack[j + 1] = 0x40;
                isoMapPack[j + 2] = 0x88;
                isoMapPack[j + 3] = 0x40;
                j += 11;
            }

            string errorMessage = ParseEncodedMapSectionData("IsoMapPack5", ref isoMapPack);

            if (errorMessage != null)
                return errorMessage;

            if (coordinateValidityLUT == null)
                CalculateCoordinateValidityLUT();

            for (int x = 0; x < coordinateValidityLUT.GetLength(0); x++)
            {
                for (int y = 0; y < coordinateValidityLUT.GetLength(1); y++)
                {
                    if (coordinateValidityLUT[x, y])
                    {
                        MapTile tile = new MapTile((short)x, (short)y);
                        tile.PropertyChanged += MapTile_PropertyChanged;
                        mapTiles.Add(new Tuple<int, int>(x, y), tile);
                    }
                }
            }

            int bytesRead = 0;

            for (int i = 0; i < cellCount; i++)
            {
                ushort x = BitConverter.ToUInt16(isoMapPack, bytesRead);
                bytesRead += 2;
                ushort y = BitConverter.ToUInt16(isoMapPack, bytesRead);
                bytesRead += 2;
                int tileNum = BitConverter.ToInt32(isoMapPack, bytesRead);
                bytesRead += 4;
                byte subTile = isoMapPack[bytesRead++];
                byte level = isoMapPack[bytesRead++];
                byte iceGrowth = isoMapPack[bytesRead++];

                if (x > 0 && y > 0 && x < 512 && y < 512)
                {
                    var key = new Tuple<int, int>(x, y);

                    if (mapTiles.ContainsKey(key))
                    {
                        MapTile tile = new MapTile((short)x, (short)y, tileNum, subTile, level, iceGrowth);
                        tile.PropertyChanged += MapTile_PropertyChanged;
                        mapTiles[key] = tile;
                    }
                }
            }

            tileDataAltered = false;
            return null;
        }

        /// <summary>
        /// Saves tile data to map file.
        /// </summary>
        private void SaveTileData()
        {
            if (mapTiles == null || !tileDataAltered)
                return;

            if (mapTiles.Count < 1)
            {
                RemoveSection("IsoMapPack5");
                return;
            }

            byte[] isoMapPack = new byte[mapTiles.Count * 11 + 4];
            int i = 0;

            foreach (KeyValuePair<Tuple<int, int>, MapTile> kvp in mapTiles)
            {
                MapTile tile = kvp.Value;
                byte[] x = BitConverter.GetBytes(tile.X);
                byte[] y = BitConverter.GetBytes(tile.Y);
                byte[] tilei = BitConverter.GetBytes(tile.TileIndex);
                isoMapPack[i] = x[0];
                isoMapPack[i + 1] = x[1];
                isoMapPack[i + 2] = y[0];
                isoMapPack[i + 3] = y[1];
                isoMapPack[i + 4] = tilei[0];
                isoMapPack[i + 5] = tilei[1];
                isoMapPack[i + 6] = tilei[2];
                isoMapPack[i + 7] = tilei[3];
                isoMapPack[i + 8] = tile.SubTileIndex;
                isoMapPack[i + 9] = tile.Level;
                isoMapPack[i + 10] = tile.IceGrowth;
                i += 11;
            }

            ReplacePackedSectionData("IsoMapPack5", MapPackHelper.CompressMapPackData(isoMapPack));
        }

        /// <summary>
        /// Loads overlay data from map file if it hasn't already been loaded.
        /// </summary>
        /// <returns>Array of error messages if something went wrong, otherwise null.</returns>
        public string[] LoadOverlayData()
        {
            if (mapOverlays != null)
                return new string[] { "Overlay data has already been loaded." };

            List<string> errorMessages = new List<string>();

            byte[] overlayPack = new byte[1 << 18];
            string errorMessage = ParseEncodedMapSectionData("OverlayPack", ref overlayPack, true);

            if (errorMessage != null)
                errorMessages.Add(errorMessage);

            byte[] overlayDataPack = new byte[1 << 18];
            errorMessage = ParseEncodedMapSectionData("OverlayDataPack", ref overlayDataPack, true);

            if (errorMessage != null)
                errorMessages.Add(errorMessage);

            if (errorMessages.Count < 1)
            {
                mapOverlays = new List<MapOverlay>(overlayPack.Length);
                for (int i = 0; i < overlayPack.Length; i++)
                {
                    byte index = overlayPack[i];
                    byte frame = overlayDataPack[i];
                    short x = (short)(i % 512);
                    short y = (short)((i - x) / 512);
                    MapOverlay overlay = new MapOverlay(x, y, index, frame);
                    overlay.PropertyChanged += MapOverlay_PropertyChanged;
                    mapOverlays.Add(overlay);
                }
                overlayDataAltered = false;
            }

            return errorMessages.Count > 0 ? errorMessages.ToArray() : null;
        }

        /// <summary>
        /// Saves Overlay(Data)Pack sections of the map file.
        /// </summary>
        private void SaveOverlayData()
        {
            if (mapOverlays == null || !overlayDataAltered)
                return;

            byte[] overlayPack = new byte[mapOverlays.Count];
            byte[] overlayDataPack = new byte[mapOverlays.Count];

            for (int i = 0; i < mapOverlays.Count; i++)
            {
                overlayPack[i] = mapOverlays[i].Index;
                overlayDataPack[i] = mapOverlays[i].FrameIndex;
            }

            string base64_overlayPack = MapPackHelper.CompressMapPackData(overlayPack, true);
            string base64_overlayDataPack = MapPackHelper.CompressMapPackData(overlayDataPack, true);
            ReplacePackedSectionData("OverlayPack", base64_overlayPack);
            ReplacePackedSectionData("OverlayDataPack", base64_overlayDataPack);
        }

        /// <summary>
        /// Gets current map preview as a bitmap.
        /// </summary>
        /// <returns>Map preview as a bitmap. Null if the preview could not be loaded.</returns>
        public Bitmap GetMapPreview()
        {
            if (previewBitmap != null)
                return previewBitmap;

            string[] keys = GetKeys("PreviewPack");

            if (keys == null || keys.Length == 0)
                return null;

            int previewWidth = PreviewWidth;
            int previewHeight = PreviewHeight;

            if (previewWidth < 1 || previewHeight < 1)
                return null;

            StringBuilder sb = new StringBuilder();

            foreach (string key in keys)
                sb.Append(GetKey("PreviewPack", key, string.Empty));

            byte[] dataDest = new byte[previewWidth * previewHeight * 3];

            string errorMsg = MapPackHelper.ParseMapPackData(sb.ToString(), ref dataDest);

            if (errorMsg != null)
                return null;

            previewBitmap = GraphicsUtils.CreateBitmapFromImageData(previewWidth, previewHeight, dataDest);
            return previewBitmap;
        }

        /// <summary>
        /// Replaces current map preview with a provided bitmap image.
        /// </summary>
        /// <returns>True if preview image was successfully replaced, otherwise false.</returns>
        public bool SetMapPreview(Bitmap preview)
        {
            if (preview == null)
                return false;

            int previewWidth = preview.Width;
            int previewHeight = preview.Height;

            if (previewWidth < 1 || previewHeight < 1)
                return false;

            bool success = GraphicsUtils.TryConvertBitmap(preview, out Bitmap convertedPreview, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            if (!success)
                return false;

            byte[] imageData = GraphicsUtils.GetRawImageDataFromBitmap(convertedPreview);

            if (imageData == null)
                return false;

            ReplacePackedSectionData("PreviewPack", MapPackHelper.CompressMapPackData(imageData));

            PreviewWidth = previewWidth;
            PreviewHeight = previewHeight;
            previewBitmap = convertedPreview;

            return true;
        }

        /// <summary>
        /// Parses and decompresses Base64-encoded and compressed data from specified section of map file.
        /// </summary>
        /// <param name="sectionName">Name of the section.</param>
        /// <param name="outputData">Array to put the decompressed data to.</param>
        /// <param name="useLCW">If set to true, treat data as LCW-compressed instead of LZO.</param>
        /// <returns>Error message if something went wrong, otherwise null.</returns>
        private string ParseEncodedMapSectionData(string sectionName, ref byte[] outputData, bool useLCW = false)
        {
            Logger.Info("Parsing " + sectionName + ".");
            string[] values = GetValues(sectionName);

            string msgStart = "Error parsing " + sectionName + ": ";

            if (values == null || values.Length < 1)
                return msgStart + "Data is empty.";

            string errorMsg = MapPackHelper.ParseMapPackData(values, ref outputData, useLCW);

            if (errorMsg != null)
                return msgStart + errorMsg;

            return null;
        }

        /// <summary>
        /// Replaces contents of a Base64-encoded section of map file.
        /// </summary>
        /// <param name="sectionName">Name of the section.</param>
        /// <param name="data">Contents to replace the existing contents with.</param>
        private void ReplacePackedSectionData(string sectionName, string data)
        {
            int lx = 70;
            List<string> lines = new List<string>();

            for (int x = 0; x < data.Length; x += lx)
            {
                lines.Add(data.Substring(x, Math.Min(lx, data.Length - x)));
            }

            ReplaceSectionWithStrings(sectionName, lines);
        }

        #endregion

        #region map_tiles

        /// <summary>
        /// Returns all map tiles.
        /// </summary>
        /// <returns>All map tiles.</returns>
        public MapTile[] GetMapTiles()
        {
            if (mapTiles == null)
                return new MapTile[0];

            return mapTiles.Values.ToArray();
        }

        /// <summary>
        /// Remove a single map tile from the map.
        /// </summary>
        /// <param name="mapTile">Map tile to remove.</param>
        /// <returns>True if map tile was found and removed, otherwise false.</returns>
        public bool RemoveMapTile(MapTile mapTile)
        {
            if (!HasTileData || !mapTiles.ContainsValue(mapTile))
                return false;

            if (mapTiles.Remove(new Tuple<int, int>(mapTile.X, mapTile.Y)))
            {
                tileDataAltered = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes multiple map tiles from the map.
        /// </summary>
        /// <param name="mapTiles">Collection of map tiles to remove.</param>
        /// <returns>Number of map tiles removed.</returns>
        public int RemoveMapTiles(IEnumerable<MapTile> mapTiles)
        {
            if (!HasTileData)
                return 0;

            int numberRemoved = 0;

            foreach (MapTile tile in mapTiles)
            {
                if (RemoveMapTile(tile))
                    numberRemoved++;
            }

            return numberRemoved;
        }

        /// <summary>
        /// Replaces map tile with another assuming tile coordinates match an existing map tile.
        /// </summary>
        /// <param name="mapTile">Map tile to replace existing map tile with.</param>
        /// <returns>True if map tile was replaced, otherwise false.</returns>
        public bool ReplaceMapTile(MapTile mapTile)
        {
            if (HasTileData && mapTiles.ContainsValue(mapTile))
            {
                mapTiles[new Tuple<int, int>(mapTile.X, mapTile.Y)] = mapTile;
                mapTile.PropertyChanged -= MapTile_PropertyChanged;
                mapTile.PropertyChanged += MapTile_PropertyChanged;
                tileDataAltered = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Replaces multiple map tiles assuming tile coordinates match existing map tiles.
        /// </summary>
        /// <param name="mapTiles">Collection of map tiles to replace existing map tiles with.</param>
        /// <returns>Number of map tiles replaced.</returns>
        public int ReplaceMapTiles(IEnumerable<MapTile> mapTiles)
        {
            int numberReplaced = 0;

            foreach (MapTile mapTile in mapTiles)
            {
                if (ReplaceMapTile(mapTile))
                    numberReplaced++;
            }

            return numberReplaced;
        }

        /// <summary>
        /// Sorts map tile data based on provided map tile properties, in the order listed.
        /// </summary>
        /// <param name="propertyInfos">Map tile properties.</param>
        /// <returns>True if map tiles were sorted, otherwise false.</returns>
        public bool SortMapTileDataByProperties(IList<PropertyInfo> propertyInfos)
        {
            if (!HasTileData)
                return false;

            IOrderedEnumerable<KeyValuePair<Tuple<int, int>, MapTile>> sortedTiles = null;
            int index = 0;

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if (propertyInfo == null)
                    continue;

                if (index == 0)
                {
                    sortedTiles = mapTiles.OrderBy(x => propertyInfo.GetValue(x.Value, null));
                }
                else if (sortedTiles != null)
                {
                    sortedTiles = sortedTiles.ThenBy(x => propertyInfo.GetValue(x.Value, null));
                }

                index++;
            }

            if (sortedTiles != null)
            {
                mapTiles = sortedTiles.ToDictionary(x => x.Key, x => x.Value);
                tileDataAltered = true;
            }
            else
                return false;

            return true;
        }

        #endregion

        #region map_overlays

        /// <summary>
        /// Returns all map overlays.
        /// </summary>
        /// <returns>All map overlay.</returns>
        public MapOverlay[] GetMapOverlays()
        {
            if (mapOverlays == null)
                return new MapOverlay[0];

            return mapOverlays.ToArray();
        }

        /// <summary>
        /// Remove a single map overlay from the map.
        /// </summary>
        /// <param name="mapOverlay">Map overlay to remove.</param>
        /// <returns>True if map overlay was found and removed, otherwise false.</returns>
        public bool RemoveMapOverlay(MapOverlay mapOverlay)
        {
            if (HasOverlayData && mapOverlays.Remove(mapOverlay))
            {
                overlayDataAltered = true;
                return true;
            }

            return false;
        }


        /// <summary>
        /// Removes multiple map overlays from the map.
        /// </summary>
        /// <param name="mapOverlays">Collection of map overlays to remove.</param>
        /// <returns>Number of map overlays removed.</returns>
        public int RemoveMapOverlays(IEnumerable<MapOverlay> mapOverlays)
        {
            if (!HasOverlayData)
                return 0;

            int numberRemoved = this.mapOverlays.RemoveAll(x => mapOverlays.Contains(x));

            if (numberRemoved > 0)
                overlayDataAltered = true;

            return numberRemoved;
        }

        /// <summary>
        /// Replaces map overlay with another assuming tile coordinates match an existing map overlay.
        /// </summary>
        /// <param name="mapOverlay">Map overlay to replace existing map overlay with.</param>
        /// <returns>True if map overlay was replaced, otherwise false.</returns>
        public bool ReplaceMapOverlay(MapOverlay mapOverlay)
        {
            if (HasOverlayData)
            {
                int index = mapOverlays.FindIndex(t => t.X == mapOverlay.X && t.Y == mapOverlay.Y);

                if (index < 0)
                    return false;

                mapOverlays[index] = mapOverlay;
                mapOverlay.PropertyChanged -= MapOverlay_PropertyChanged;
                mapOverlay.PropertyChanged += MapOverlay_PropertyChanged;
                tileDataAltered = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Replaces multiple map overlays assuming tile coordinates match existing map overlays.
        /// </summary>
        /// <param name="mapOverlays">Collection of map overlays to replace existing map overlays with.</param>
        /// <returns>Number of map overlays replaced.</returns>
        public int ReplaceMapOverlays(IEnumerable<MapOverlay> mapOverlays)
        {
            int numberReplaced = 0;

            foreach (MapOverlay mapOverlay in mapOverlays)
            {
                if (ReplaceMapOverlay(mapOverlay))
                    numberReplaced++;
            }

            return numberReplaced;
        }

        #endregion

        #region map_objects

        /// <summary>
        /// Gets map aircraft objects.
        /// </summary>
        /// <param name="sectionName">Name of section.</param>
        /// <returns>Dictionary containing map aircraft object keys & map aircraft objects. Will be empty if no aircraft objects are defined on the map.</returns>
        public Dictionary<string, MapAircraftObject> GetMapAircraft()
            => GetMapObjectsFromSection("Aircraft").ToDictionary(kvp => kvp.Key, kvp => kvp.Value as MapAircraftObject);

        /// <summary>
        /// Gets map building objects.
        /// </summary>
        /// <param name="sectionName">Name of section.</param>
        /// <returns>Dictionary containing map building object keys & map building objects. Will be empty if no building objects are defined on the map.</returns>
        public Dictionary<string, MapBuildingObject> GetMapBuildings()
            => GetMapObjectsFromSection("Structures").ToDictionary(kvp => kvp.Key, kvp => kvp.Value as MapBuildingObject);

        /// <summary>
        /// Gets map infantry objects.
        /// </summary>
        /// <param name="sectionName">Name of section.</param>
        /// <returns>Dictionary containing map infantry object keys & map infantry objects. Will be empty if no infantry objects are defined on the map.</returns>
        public Dictionary<string, MapInfantryObject> GetMapInfantry()
            => GetMapObjectsFromSection("Infantry").ToDictionary(kvp => kvp.Key, kvp => kvp.Value as MapInfantryObject);

        /// <summary>
        /// Gets map vehicle objects.
        /// </summary>
        /// <param name="sectionName">Name of section.</param>
        /// <returns>Dictionary containing map vehicle object keys & map vehicle objects. Will be empty if no vehicle objects are defined on the map.</returns>
        public Dictionary<string, MapVehicleObject> GetMapVehicles()
            => GetMapObjectsFromSection("Units").ToDictionary(kvp => kvp.Key, kvp => kvp.Value as MapVehicleObject);

        /// <summary>
        /// Gets map vehicle objects.
        /// </summary>
        /// <param name="sectionName">Name of section.</param>
        /// <returns>Dictionary containing map vehicle object keys & map vehicle objects. Will be empty if no vehicle objects are defined on the map.</returns>
        public Dictionary<string, MapTerrainObject> GetMapTerrainObjects()
            => GetMapObjectsFromSection("Terrain").ToDictionary(kvp => kvp.Key, kvp => kvp.Value as MapTerrainObject);

        /// <summary>
        /// Gets map objects from specified section.
        /// </summary>
        /// <param name="sectionName">Name of section.</param>
        /// <returns>Dictionary containing map object keys & map objects. Will be empty if section does not exist or does not contain valid map objects.</returns>
        public Dictionary<string, MapObject> GetMapObjectsFromSection(string sectionName)
        {
            Dictionary<string, MapObject> objects = new Dictionary<string, MapObject>();

            var keys = GetKeys(sectionName);

            if (string.IsNullOrEmpty(sectionName) || keys == null)
                return objects;

            foreach (string key in keys)
            {
                MapObject mapObject = ParseMapObject(sectionName, key);

                if (mapObject != null && mapObject.Initialized)
                    objects.Add(key, mapObject);
            }

            return objects;
        }

        /// <summary>
        /// Parse map objects.
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        /// <param name="key">Object INI key.</param>
        /// <returns>Parsed map object, or null if map object could not be parsed.</returns>
        private MapObject ParseMapObject(string sectionName, string key)
        {
            string objectDeclaration = GetKey(sectionName, key, "");

            if (string.IsNullOrEmpty(objectDeclaration))
                return null;

            switch (sectionName)
            {
                case "Aircraft":
                    return new MapAircraftObject(objectDeclaration);
                case "Infantry":
                    return new MapInfantryObject(objectDeclaration);
                case "Structures":
                    return new MapBuildingObject(objectDeclaration);
                case "Units":
                    return new MapVehicleObject(objectDeclaration);
                case "Terrain":
                    return new MapTerrainObject(key, objectDeclaration);
                default:
                    return null;
            }
        }

        #endregion

        #region events

        private void MapTile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (tileDataAltered || !(sender is MapTile))
                return;

            if (mapTiles.ContainsValue(sender as MapTile))
                tileDataAltered = true;
        }

        private void MapOverlay_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (overlayDataAltered || !(sender is MapOverlay))
                return;

            if (mapOverlays.Contains(sender as MapOverlay))
                overlayDataAltered = true;
        }

        #endregion
    }
}
