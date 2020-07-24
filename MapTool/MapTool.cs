/*
 * Copyright 2017-2020 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using MapTool.DataStructures;
using StarkkuUtils.FileTypes;
using StarkkuUtils.Utilities;
using StarkkuUtils.ExtensionMethods;

namespace MapTool
{
    /// <summary>
    /// Map tile data sort mode.
    /// </summary>
    public enum IsoMapPack5SortMode { NotDefined, XLevelTileIndex, XTileIndexLevel, TileIndexXLevel, LevelXTileIndex, X, Level, TileIndex, SubTileIndex, IceGrowth, Y }

    /// <summary>
    /// Map file modifier tool class.
    /// </summary>
    public class MapTool
    {
        #region public_properties

        /// <summary>
        /// Has tool been initialized or not.
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// Has map file been altered or not.
        /// </summary>
        public bool MapAltered { get; private set; }

        /// <summary>
        /// Map input filename.
        /// </summary>
        public string FilenameInput { get; private set; }

        /// <summary>
        /// Map output filename.
        /// </summary>
        public string FilenameOutput { get; private set; }

        #endregion

        #region private_fields

        /// <summary>
        /// Map file.
        /// </summary>
        private readonly INIFile mapINI;

        /// <summary>
        /// Map theater.
        /// </summary>
        private readonly string mapTheater = null;

        /// <summary>
        /// Map full width.
        /// </summary>
        private readonly int mapWidth;

        /// <summary>
        /// Map full height.
        /// </summary>
        private readonly int mapHeight;

        /// <summary>
        /// Map tile data.
        /// </summary>
        private List<MapTile> isoMapPack5 = new List<MapTile>();

        /// <summary>
        /// Map overlay ID data.
        /// </summary>
        private byte[] overlayPack = null;

        /// <summary>
        /// Map overlay frame data.
        /// </summary>
        private byte[] overlayDataPack = null;

        /// <summary>
        /// Look-up table for map coordinate (X,Y) validity.
        /// </summary>
        private bool[,] coordinateValidityLUT;

        /// <summary>
        /// Conversion profile INI file.
        /// </summary>
        private readonly INIFile conversionProfileINI;

        /// <summary>
        /// Conversion profile applicable theaters.
        /// </summary>
        private readonly List<string> applicableTheaters = new List<string>();

        /// <summary>
        /// Conversion profile theater-specific global tile offsets.
        /// </summary>
        private readonly Dictionary<string, Tuple<int, int>> theaterTileOffsets = new Dictionary<string, Tuple<int, int>>();

        /// <summary>
        /// Conversion profile new theater.
        /// </summary>
        private readonly string newTheater = null;

        /// <summary>
        /// Conversion profile tile rules.
        /// </summary>
        private readonly List<TileConversionRule> tileRules = new List<TileConversionRule>();

        /// <summary>
        /// Conversion profile overlay rules.
        /// </summary>
        private readonly List<OverlayConversionRule> overlayRules = new List<OverlayConversionRule>();

        /// <summary>
        /// Conversion profile object rules.
        /// </summary>
        private readonly List<StringConversionRule> objectRules = new List<StringConversionRule>();

        /// <summary>
        /// // Conversion profile section rules.
        /// </summary>
        private readonly List<SectionConversionRule> sectionRules = new List<SectionConversionRule>();

        /// <summary>
        /// Optimize output map file or not.
        /// </summary>
        private readonly bool useMapOptimize = false;

        /// <summary>
        /// Compress output map file or not.
        /// </summary>
        private readonly bool useMapCompress = false;

        /// <summary>
        /// Delete objects outside visible map bounds or not.
        /// </summary>
        private readonly bool deleteObjectsOutsideMapBounds = false;

        /// <summary>
        /// Remove clear tiles at level 0 from map tile data (they will be filled in by the game) or not.
        /// </summary>
        private readonly bool removeLevel0ClearTiles = false;

        /// <summary>
        /// Fix tunnel data or not.
        /// </summary>
        private readonly bool fixTunnels = false;

        /// <summary>
        /// Map tile data sort mode.
        /// </summary>
        private readonly IsoMapPack5SortMode isoMapPack5SortBy = IsoMapPack5SortMode.NotDefined;

        /// <summary>
        /// Theater configuration file.
        /// </summary>
        private readonly INIFile theaterConfigINI;

        /// <summary>
        /// OverlayPack/DataPack length.
        /// </summary>
        private const int OVERLAY_DATA_LENGTH = 262144;

        /// <summary>
        /// Random number generator.
        /// </summary>
        private readonly Random random = new Random();

        #endregion

        /// <summary>
        /// Initializes a new instance of MapTool.
        /// </summary>
        /// <param name="inputFile">Input file name.</param>
        /// <param name="outputFile">Output file name.</param>
        /// <param name="fileConfig">Conversion profile file name.</param>
        /// <param name="listTheaterData">If set, it is assumed that this instance of MapTool is initialized for listing theater data rather than processing maps.</param>
        public MapTool(string inputFile, string outputFile, string fileConfig, bool listTheaterData)
        {
            Initialized = false;
            MapAltered = false;
            FilenameInput = inputFile;
            FilenameOutput = outputFile;

            if (listTheaterData && !string.IsNullOrEmpty(FilenameInput))
            {
                theaterConfigINI = new INIFile(FilenameInput);
            }

            else if (!string.IsNullOrEmpty(FilenameInput) && !string.IsNullOrEmpty(FilenameOutput))
            {

                Logger.Info("Reading map file '" + inputFile + "'.");
                mapINI = new INIFile(inputFile);

                string[] size = mapINI.GetKey("Map", "Size", "").Split(',');
                mapWidth = Conversion.GetIntFromString(size[2], -1);
                mapHeight = Conversion.GetIntFromString(size[3], -1);

                mapTheater = mapINI.GetKey("Map", "Theater", null);
                if (mapTheater != null)
                    mapTheater = mapTheater.ToUpper();

                Logger.Info("Parsing conversion profile file.");
                conversionProfileINI = new INIFile(fileConfig);
                string[] sections = conversionProfileINI.GetSections();
                if (sections == null || sections.Length < 1)
                {
                    Logger.Error("Conversion profile file is empty.");
                    Initialized = false;
                    return;
                }

                string include = conversionProfileINI.GetKey("ProfileData", "IncludeFiles", null);
                if (!string.IsNullOrEmpty(include))
                {
                    string[] includeFiles = include.Split(',');
                    string basedir = Path.GetDirectoryName(fileConfig);
                    foreach (string filename in includeFiles)
                    {
                        if (File.Exists(basedir + "\\" + filename))
                        {
                            INIFile includeIni = new INIFile(basedir + "\\" + filename);
                            Logger.Info("Merging included file '" + filename + "' to conversion profile.");
                            conversionProfileINI.Merge(includeIni);
                        }
                    }
                }

                // Parse general options.
                useMapOptimize = Conversion.GetBoolFromString(conversionProfileINI.GetKey("ProfileData", "ApplyMapOptimization", "false"), false);
                useMapCompress = Conversion.GetBoolFromString(conversionProfileINI.GetKey("ProfileData", "ApplyMapCompress", "false"), false);
                deleteObjectsOutsideMapBounds = Conversion.GetBoolFromString(conversionProfileINI.GetKey("ProfileData", "DeleteObjectsOutsideMapBounds",
                    "false"), false);
                fixTunnels = Conversion.GetBoolFromString(conversionProfileINI.GetKey("ProfileData", "FixTunnels", "false"), false);

                // Parse tile data options.
                string sortMode = conversionProfileINI.GetKey("IsoMapPack5", "SortBy", null);
                if (sortMode != null)
                {
                    Enum.TryParse(sortMode.Replace("_", ""), true, out isoMapPack5SortBy);
                }
                removeLevel0ClearTiles = Conversion.GetBoolFromString(conversionProfileINI.GetKey("IsoMapPack5", "RemoveLevel0ClearTiles", "false"), false);

                // Parse theater rules.
                newTheater = conversionProfileINI.GetKey("TheaterRules", "NewTheater", null);
                if (newTheater != null)
                    newTheater = newTheater.ToUpper();

                string[] applicableTheaters = conversionProfileINI.GetKey("TheaterRules", "ApplicableTheaters", "").Split(',');
                if (applicableTheaters != null)
                {
                    for (int i = 0; i < applicableTheaters.Length; i++)
                    {
                        string theater = applicableTheaters[i].Trim().ToUpper();
                        if (theater == "")
                            continue;
                        this.applicableTheaters.Add(theater);
                    }
                }

                if (this.applicableTheaters.Count < 1)
                    this.applicableTheaters.AddRange(new string[] { "TEMPERATE", "SNOW", "URBAN", "DESERT", "LUNAR", "NEWURBAN" });

                // Parse theater-specific global tile offsets.
                string[] theaterOffsetKeys = conversionProfileINI.GetKeys("TheaterTileOffsets");
                if (theaterOffsetKeys != null)
                {
                    foreach (string key in theaterOffsetKeys)
                    {
                        int newOffset = int.MinValue;
                        string[] values = conversionProfileINI.GetKey("TheaterTileOffsets", key, "").Split(',');
                        int originalOffset;
                        if (values.Length < 1)
                            continue;
                        else if (values.Length < 2)
                        {
                            originalOffset = Conversion.GetIntFromString(values[0], 0);
                        }
                        else
                        {
                            originalOffset = Conversion.GetIntFromString(values[0], 0);
                            newOffset = Conversion.GetIntFromString(values[1], int.MinValue);
                        }
                        theaterTileOffsets.Add(key.ToUpper(), new Tuple<int, int>(originalOffset, newOffset));
                    }
                }

                // Parse conversion rules.
                string[] tilerules = conversionProfileINI.GetKeys("TileRules");
                string[] overlayrules = conversionProfileINI.GetKeys("OverlayRules");
                string[] objectrules = conversionProfileINI.GetKeys("ObjectRules");
                string[] sectionrules = MergeKeyValuePairs(conversionProfileINI.GetKeyValuePairs("SectionRules"));

                // Allow saving map without any other changes if either of these are set and ApplicableTheaters allows it.
                bool allowSaving = (useMapCompress || useMapOptimize || deleteObjectsOutsideMapBounds || fixTunnels ||
                    isoMapPack5SortBy != IsoMapPack5SortMode.NotDefined) && IsCurrentTheaterAllowed();

                if (!allowSaving && tilerules == null && overlayrules == null && objectrules == null && sectionrules == null &&
                    string.IsNullOrEmpty(newTheater))
                {
                    Logger.Error("No conversion rules to apply in the conversion profile file.");
                    Initialized = false;
                    return;
                }

                ParseConversionRules(tilerules, tileRules);
                ParseConversionRules(overlayrules, overlayRules);
                ParseConversionRules(objectrules, objectRules);
                ParseConversionRules(sectionrules, sectionRules);
            }

            Initialized = true;
        }

        /// <summary>
        /// Saves the map file.
        /// </summary>
        public void Save()
        {
            if (deleteObjectsOutsideMapBounds)
            {
                if (mapWidth > 0 && mapHeight > 0)
                {
                    Logger.Info("DeleteObjectsOutsideMapBounds set: Objects & overlays outside map bounds will be deleted.");
                    CalculateCoordinateValidity();
                    DeleteObjectsOutsideBounds();
                    DeleteOverlaysOutsideBounds();
                }
                else
                    Logger.Warn("DeleteObjectsOutsideMapBounds set but because map has invalid Size value set, no objects or overlays will be deleted.");
            }
            if (useMapOptimize)
            {
                Logger.Info("ApplyMapOptimization set: Saved map will have map section order optimizations applied.");
                mapINI.MoveSectionToFirst("Basic");
                mapINI.MoveSectionToFirst("MultiplayerDialogSettings");
                mapINI.MoveSectionToLast("Digest");
                MapAltered = true;
            }
            if (fixTunnels)
            {
                Logger.Info("FixTunnels set: Saved map will have [Tubes] section fixed to remove errors caused by map editor.");
                FixTubesSection();
            }
            if (useMapCompress)
                Logger.Info("ApplyMapCompress set: Saved map will have no unnecessary whitespaces or comments.");

            string error;
            if (MapAltered || useMapCompress)
                error = mapINI.Save(FilenameOutput, !useMapCompress, !useMapCompress);
            else
            {
                Logger.Info("Skipping saving map file as no changes have been made to it.");
                return;
            }

            if (string.IsNullOrEmpty(error))
                Logger.Info("Map file successfully saved to '" + FilenameOutput + "'.");
            else
            {
                Logger.Error("Error encountered saving map file to '" + FilenameOutput + "'.");
                Logger.Error("Message: " + error);
            }
        }

        /// <summary>
        /// Checks if theater name is valid.
        /// </summary>
        /// <param name="theaterName">Theater name.</param>
        /// <returns>True if a valid theater name, otherwise false.</returns>
        public static bool IsValidTheaterName(string theaterName)
        {
            if (theaterName == "TEMPERATE" || theaterName == "SNOW" || theaterName == "LUNAR" || theaterName == "DESERT" ||
                theaterName == "URBAN" || theaterName == "NEWURBAN")
                return true;

            return false;
        }

        /// <summary>
        /// Checks if the currently set map theater exists in current list of theaters the map tool is allowed to process.
        /// </summary>
        /// <returns>True if map theater exists in applicable theaters, otherwise false.</returns>
        private bool IsCurrentTheaterAllowed()
        {
            if (applicableTheaters == null || mapTheater == null || !applicableTheaters.Contains(mapTheater))
                return false;

            return true;
        }

        #region map_pack_handling

        /// <summary>
        /// Parses IsoMapPack5 section of the map file.
        /// </summary>
        /// <returns>Error message if something went wrong, otherwise null.</returns>
        private string ParseMapPack()
        {
            int cellCount;
            byte[] isoMapPack;

            if (mapHeight < 1 || mapWidth < 1)
                return ("Map Size is invalid.");

            cellCount = (mapWidth * 2 - 1) * mapHeight;
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
                if (x > 0 && y > 0 && x <= 16384 && y <= 16384)
                {
                    isoMapPack5.Add(new MapTile((short)x, (short)y, tileNum, subTile, level, iceGrowth));
                }
            }
            return null;
        }

        /// <summary>
        /// Saves IsoMapPack5 section of the map file.
        /// </summary>
        private void SaveMapPack()
        {
            if (!Initialized || isoMapPack5.Count < 1)
                return;

            byte[] isoMapPack = new byte[isoMapPack5.Count * 11 + 4];
            int i = 0;

            foreach (MapTile t in isoMapPack5)
            {
                byte[] x = BitConverter.GetBytes(t.X);
                byte[] y = BitConverter.GetBytes(t.Y);
                byte[] tilei = BitConverter.GetBytes(t.TileIndex);
                isoMapPack[i] = x[0];
                isoMapPack[i + 1] = x[1];
                isoMapPack[i + 2] = y[0];
                isoMapPack[i + 3] = y[1];
                isoMapPack[i + 4] = tilei[0];
                isoMapPack[i + 5] = tilei[1];
                isoMapPack[i + 6] = tilei[2];
                isoMapPack[i + 7] = tilei[3];
                isoMapPack[i + 8] = t.SubTileIndex;
                isoMapPack[i + 9] = t.Level;
                isoMapPack[i + 10] = t.IceGrowth;
                i += 11;
            }

            byte[] lzo = MapPackHelper.Compress(isoMapPack);
            string data = Convert.ToBase64String(lzo, Base64FormattingOptions.None);
            ReplacePackedSectionData("IsoMapPack5", data);
        }

        /// <summary>
        /// Parses Overlay(Data)Pack sections of the map file.
        /// </summary>
        private void ParseOverlayPacks()
        {
            overlayPack = new byte[1 << 18];
            string errorMessage = ParseEncodedMapSectionData("OverlayPack", ref overlayPack, true);
            if (errorMessage != null)
                Logger.Warn(errorMessage);

            overlayDataPack = new byte[1 << 18];
            errorMessage = ParseEncodedMapSectionData("OverlayDataPack", ref overlayDataPack, true);
            if (errorMessage != null)
                Logger.Warn(errorMessage);
        }


        /// <summary>
        /// Saves Overlay(Data)Pack sections of the map file.
        /// </summary>
        private void SaveOverlayPacks()
        {
            string base64_overlayPack = Convert.ToBase64String(MapPackHelper.Compress(overlayPack, true), Base64FormattingOptions.None);
            string base64_overlayDataPack = Convert.ToBase64String(MapPackHelper.Compress(overlayDataPack, true), Base64FormattingOptions.None);
            ReplacePackedSectionData("OverlayPack", base64_overlayPack);
            ReplacePackedSectionData("OverlayDataPack", base64_overlayDataPack);
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
            string[] values = mapINI.GetValues(sectionName);
            if (values == null || values.Length < 1)
                return sectionName + " data is empty.";
            byte[] compressedData;
            try
            {
                compressedData = Convert.FromBase64String(string.Join("", values));
            }
            catch (Exception)
            {
                return sectionName + " is malformed.";
            }
            if (MapPackHelper.Decompress(compressedData, outputData, out _, useLCW))
            {
                return null;
            }
            else
            {
                return sectionName + " data is invalid or corrupted.";
            }
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
            mapINI.ReplaceSectionKeysAndValues(sectionName, lines);
        }

        #endregion

        #region conversion_rule_parsing

        /// <summary>
        /// Parses conversion profile information for tile conversion rules.
        /// </summary>
        private void ParseConversionRules(string[] ruleStrings, List<TileConversionRule> currentRules)
        {
            if (ruleStrings == null || ruleStrings.Length < 1 || currentRules == null) return;
            currentRules.Clear();

            foreach (string ruleString in ruleStrings)
            {
                string ruleStringFiltered = GetCoordinateFilters(ruleString, out int coordFilterX, out int coordFilterY);

                string[] values = ruleStringFiltered.Split('|');

                if (values.Length < 2)
                    continue;

                ParseValueRange(values[0], out int oldValueStart, out int oldValueEnd, out bool oldValueIsRange, out _);
                ParseValueRange(values[1], out int newValueStart, out int newValueEnd, out bool newValueIsRange, out bool newValueIsRandom, true);

                int heightOverride = -1;
                int subTileOverride = -1;
                int iceGrowthOverride = -1;
                if (values.Length >= 3 && values[2] != null && !values[2].Equals("*", StringComparison.InvariantCultureIgnoreCase))
                {
                    heightOverride = Conversion.GetIntFromString(values[2], -1);
                }
                if (values.Length >= 4 && values[3] != null && !values[3].Equals("*", StringComparison.InvariantCultureIgnoreCase))
                {
                    subTileOverride = Conversion.GetIntFromString(values[3], -1);
                }
                if (values.Length >= 5 && values[4] != null && !values[4].Equals("*", StringComparison.InvariantCultureIgnoreCase))
                {
                    iceGrowthOverride = Conversion.GetIntFromString(values[4], -1);
                }

                if (oldValueIsRange && !newValueIsRange)
                {
                    int diff = newValueStart + (oldValueEnd - newValueStart);
                    currentRules.Add(new TileConversionRule(oldValueStart, newValueStart, oldValueEnd, diff, newValueIsRandom, heightOverride, subTileOverride, iceGrowthOverride, coordFilterX, coordFilterY));
                }
                else if (!oldValueIsRange && newValueIsRange)
                {
                    currentRules.Add(new TileConversionRule(oldValueStart, newValueStart, oldValueStart, newValueEnd, newValueIsRandom, heightOverride, subTileOverride, iceGrowthOverride, coordFilterX, coordFilterY));
                }
                else
                {
                    currentRules.Add(new TileConversionRule(oldValueStart, newValueStart, oldValueEnd, newValueEnd, newValueIsRandom, heightOverride, subTileOverride, iceGrowthOverride, coordFilterX, coordFilterY));
                }
            }
        }

        /// <summary>
        /// Parses conversion profile information for overlay conversion rules.
        /// </summary>
        private void ParseConversionRules(string[] ruleStrings, List<OverlayConversionRule> currentRules)
        {
            if (ruleStrings == null || ruleStrings.Length < 1 || currentRules == null) return;
            currentRules.Clear();

            foreach (string ruleString in ruleStrings)
            {
                string ruleStringFiltered = GetCoordinateFilters(ruleString, out int coordFilterX, out int coordFilterY);

                string[] values = ruleStringFiltered.Split('|');

                if (values.Length < 2)
                    continue;

                ParseValueRange(values[0], out int oldValueStart, out int oldValueEnd, out bool oldValueIsRange, out _);
                ParseValueRange(values[1], out int newValueStart, out int newValueEnd, out bool newValueIsRange, out bool newValueIsRandom, true);
                ParseValueRange(values.Length >= 4 ? values[2] : "", out int frameOldValueStart, out int frameOldValueEnd, out bool frameOldValueIsRange, out _);
                ParseValueRange(values.Length >= 4 ? values[3] : "", out int frameNewValueStart, out int frameNewValueEnd, out bool frameNewValueIsRange, out bool frameNewValueIsRandom, true);

                int frameOldEndIndex = frameOldValueEnd;
                int frameNewEndIndex = frameNewValueEnd;

                if (frameOldValueIsRange && !frameNewValueIsRange)
                {
                    frameOldEndIndex = frameOldValueEnd;
                    frameNewEndIndex = frameNewValueStart + (frameOldValueEnd - frameNewValueStart);
                }
                else if (!frameOldValueIsRange && frameNewValueIsRange)
                {
                    frameOldEndIndex = frameOldValueStart;
                    frameNewEndIndex = frameNewValueEnd;
                }

                if (oldValueIsRange && !newValueIsRange)
                {
                    int diff = newValueStart + (oldValueEnd - newValueStart);
                    currentRules.Add(new OverlayConversionRule(oldValueStart, newValueStart, oldValueEnd, diff, newValueIsRandom,
                        frameOldValueStart, frameNewValueStart, frameOldEndIndex, frameNewEndIndex, frameNewValueIsRandom, coordFilterX, coordFilterY));
                }
                else if (!oldValueIsRange && newValueIsRange)
                {
                    currentRules.Add(new OverlayConversionRule(oldValueStart, newValueStart, oldValueStart, newValueEnd, newValueIsRandom,
                        frameOldValueStart, frameNewValueStart, frameOldEndIndex, frameNewEndIndex, frameNewValueIsRandom, coordFilterX, coordFilterY));
                }
                else
                {
                    currentRules.Add(new OverlayConversionRule(oldValueStart, newValueStart, oldValueEnd, newValueEnd, newValueIsRandom,
                        frameOldValueStart, frameNewValueStart, frameOldEndIndex, frameNewEndIndex, frameNewValueIsRandom, coordFilterX, coordFilterY));
                }
            }
        }

        /// <summary>
        /// Parses a value range for byte ID-type conversion rules from string.
        /// </summary>
        /// <param name="value">String from which the value will be parsed.</param>
        /// <param name="valueA">Will be set to the first value of value range.</param>
        /// <param name="valueB">Will be set to the second value of value range.</param>
        /// <param name="isRange">Will be set to true if value range truly is a range of values, false otherwise.</param>
        /// <param name="isRandom">Will be set to true if value range is a randomized range, false otherwise.</param>
        /// <param name="allowRandomRange">If set to true, allows parsing of random value ranges.</param>
        /// <returns>True is value range was completely parsed, false otherwise.</returns>
        private bool ParseValueRange(string value, out int valueA, out int valueB, out bool isRange, out bool isRandom, bool allowRandomRange = false)
        {
            valueB = -1;
            isRange = false;
            isRandom = false;

            if (allowRandomRange && value.Contains('~'))
            {
                isRange = true;
                isRandom = true;
                string[] parts = value.Split('~');
                valueA = Conversion.GetIntFromString(parts[0], -1);
                valueB = Conversion.GetIntFromString(parts[1], -1);
                if (valueA < 0 || valueB < 0)
                    return false;
            }
            else if (value.Contains('-'))
            {
                isRange = true;
                string[] parts = value.Split('-');
                valueA = Conversion.GetIntFromString(parts[0], -1);
                valueB = Conversion.GetIntFromString(parts[1], -1);
                if (valueA < 0 || valueB < 0)
                    return false;
            }
            else
            {
                valueA = Conversion.GetIntFromString(value, -1);
                if (valueA < 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Parses conversion profile information for string ID-type rules.
        /// </summary>
        private void ParseConversionRules(string[] ruleStrings, List<StringConversionRule> currentRules)
        {
            if (ruleStrings == null || ruleStrings.Length < 1 || currentRules == null)
                return;

            currentRules.Clear();

            foreach (string ruleString in ruleStrings)
            {
                string[] values = ruleString.Split('|');
                if (values.Length == 1)
                    currentRules.Add(new StringConversionRule(values[0], null));
                else if (values.Length >= 2)
                    currentRules.Add(new StringConversionRule(values[0], values[1]));
            }
        }

        /// <summary>
        /// Parses conversion profile information for map file section rules.
        /// </summary>
        private void ParseConversionRules(string[] ruleStrings, List<SectionConversionRule> currentRules)
        {
            if (ruleStrings == null || ruleStrings.Length < 1 || currentRules == null)
                return;

            currentRules.Clear();

            foreach (string ruleString in ruleStrings)
            {
                if (ruleString == null || ruleString.Length < 1)
                    continue;
                string[] values = ruleString.Split('|');
                string newSection = "";
                string originalKey = "";
                string newKey = "";
                string newValue = "";
                if (values.Length > 0)
                {
                    if (values[0].StartsWith("="))
                        values[0] = values[0].Substring(1, values[0].Length - 1);
                    string[] sec = values[0].Split('=');
                    if (sec == null || sec.Length < 1)
                        continue;
                    string originalSection = sec[0];
                    if (sec.Length == 1 && values[0].Contains('=') || sec.Length > 1 && values[0].Contains('=') &&
                        string.IsNullOrEmpty(sec[1]))
                        newSection = null;
                    else if (sec.Length > 1)
                        newSection = sec[1];
                    if (values.Length > 1)
                    {
                        string[] key = values[1].Split('=');
                        if (key != null && key.Length > 0)
                        {
                            originalKey = key[0];
                            if (key.Length == 1 && values[1].Contains('=') || key.Length > 1 && values[1].Contains('=') &&
                                string.IsNullOrEmpty(key[1])) newKey = null;
                            else if (key.Length > 1) newKey = key[1];
                        }
                        if (values.Length > 2)
                        {
                            if (!(values[2] == null || values[2] == "" || values[2] == "*"))
                            {
                                if (values[2].Contains("$GETVAL("))
                                {
                                    string[] valdata = Regex.Match(values[2], @"\$GETVAL\(([^)]*)\)").Groups[1].Value.Split(',');
                                    if (valdata.Length > 1)
                                    {
                                        string newval = mapINI.GetKey(valdata[0], valdata[1], null);
                                        if (newval != null)
                                        {
                                            newValue = newval;
                                            if (valdata.Length > 3)
                                            {
                                                bool useDouble = true;
                                                if (valdata.Length > 4)
                                                    useDouble = Conversion.GetBoolFromString(valdata[4], true);
                                                newValue = ApplyArithmeticOp(newValue, valdata[2], valdata[3], useDouble);
                                            }
                                        }

                                    }
                                }
                                else
                                    newValue = values[2];
                            }
                        }
                    }
                    currentRules.Add(new SectionConversionRule(originalSection, newSection, originalKey, newKey, newValue));
                }
            }
        }

        private string ApplyArithmeticOp(string value, string opType, string operand, bool useDouble)
        {
            bool valueAvailable = double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double valueDouble);
            bool operandAvailable = double.TryParse(operand, NumberStyles.Number, CultureInfo.InvariantCulture, out double operandDouble);

            if (valueAvailable)
            {
                switch (opType)
                {
                    case "+":
                        valueDouble += operandDouble;
                        break;
                    case "-":
                        valueDouble -= operandDouble;
                        break;
                    case "*":
                        if (!operandAvailable)
                            operandDouble = 1;
                        valueDouble = valueDouble * operandDouble;
                        break;
                    case "/":
                        if (operandDouble == 0)
                            operandDouble = 1;
                        valueDouble = valueDouble / operandDouble;
                        break;
                }
                if (useDouble)
                    return valueDouble.ToString(CultureInfo.InvariantCulture);
                else
                    return ((int)valueDouble).ToString();
            }
            return value;
        }

        /// <summary>
        /// Gets coordinate filters from a conversion rule string and returns it without the filter part.
        /// </summary>
        /// <param name="ruleString">Rule string.</param>
        /// <param name="coordFilterX">Filter coordinate X.</param>
        /// <param name="coordFilterY">Filter coordinate Y.</param>
        /// <returns>Rule string without coordinate filters.</returns>
        private string GetCoordinateFilters(string ruleString, out int coordFilterX, out int coordFilterY)
        {
            string ruleStringFiltered = ruleString;
            coordFilterX = -1;
            coordFilterY = -1;

            if (ruleStringFiltered.StartsWith("(") && ruleStringFiltered.Contains(")"))
            {
                string coordString = ruleStringFiltered.Substring(1, ruleStringFiltered.IndexOf(")") - 1).Replace("*", -1 + "");
                string[] coords = coordString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (coords.Length >= 2)
                {
                    coordFilterX = Conversion.GetIntFromString(coords[0], -1);
                    coordFilterY = Conversion.GetIntFromString(coords[1], -1);
                }
                ruleStringFiltered = ruleStringFiltered.ReplaceFirst("(" + coordString + ")", "");
            }
            return ruleStringFiltered;
        }

        #endregion

        #region conversion

        /// <summary>
        /// Changes theater declaration of current map based on conversion profile.
        /// </summary>
        public void ConvertTheaterData()
        {
            if (!Initialized || string.IsNullOrEmpty(newTheater))
                return;
            else if (!IsCurrentTheaterAllowed())
            {
                Logger.Warn("Skipping altering theater data - ApplicableTheaters does not contain entry matching map theater.");
                return;
            }

            Logger.Info("Attempting to modify theater data of the map file.");

            if (IsValidTheaterName(newTheater))
            {
                mapINI.SetKey("Map", "Theater", newTheater);
                Logger.Info("Map theater declaration changed from '" + mapTheater + "' to '" + newTheater + "'.");
                MapAltered = true;
            }
        }

        /// <summary>
        /// Changes tile data of current map based on conversion profile.
        /// </summary>
        public void ConvertTileData()
        {
            if (!Initialized)
                return;

            if (tileRules.Count > 0 || removeLevel0ClearTiles || isoMapPack5SortBy != IsoMapPack5SortMode.NotDefined)
            {
                string errorMessage = ParseMapPack();
                if (errorMessage != null)
                {
                    Logger.Error("Error encountered parsing tile data: Message: " + errorMessage);
                    return;
                }
            }

            if (isoMapPack5.Count < 1)
                return;

            else if (!IsCurrentTheaterAllowed())
            {
                Logger.Warn("Skipping altering tile data - ApplicableTheaters does not contain entry matching map theater.");
                return;
            }

            bool tileDataAltered = ApplyTileConversionRules();
            tileDataAltered |= RemoveLevel0ClearTiles() || tileDataAltered;
            tileDataAltered |= SortIsoMapPack() || tileDataAltered;

            if (tileDataAltered)
                SaveMapPack();

            MapAltered |= tileDataAltered;
        }

        /// <summary>
        /// Processes tile data conversion rules.
        /// </summary>
        /// <returns>Returns true if tile data was changed, false if not.</returns>
        private bool ApplyTileConversionRules()
        {
            if (tileRules == null || tileRules.Count < 1)
                return false;

            Logger.Info("Attempting to apply TileRules on map tile data.");

            int originalOffset = 0, newOffset = 0;
            bool tileDataAltered = false;

            if (theaterTileOffsets.ContainsKey(mapTheater.ToUpper()))
            {
                originalOffset = theaterTileOffsets[mapTheater.ToUpper()].Item1;
                newOffset = theaterTileOffsets[mapTheater.ToUpper()].Item2;
                if (newOffset == int.MinValue)
                    newOffset = originalOffset;
                if (originalOffset != 0 && newOffset != 0)
                    Logger.Info("Global tile rule offsets for theater " + mapTheater.ToUpper() + ": " + originalOffset + " (original), " + newOffset + " (new)");
            }

            foreach (MapTile tile in isoMapPack5)
            {
                if (tile.TileIndex < 0 || tile.TileIndex == 65535)
                    tile.TileIndex = 0;

                foreach (TileConversionRule rule in tileRules)
                {
                    if (rule.CoordinateFilterX > -1 && rule.CoordinateFilterX != tile.X ||
                        rule.CoordinateFilterY > -1 && rule.CoordinateFilterY != tile.Y)
                        continue;

                    int ruleOriginalStartIndex = rule.OriginalStartIndex + originalOffset;
                    int ruleOriginalEndIndex = rule.OriginalEndIndex + originalOffset;
                    int ruleNewStartIndex = rule.NewStartIndex + newOffset;
                    int ruleNewEndIndex = rule.NewEndIndex + newOffset;

                    if (tile.TileIndex >= ruleOriginalStartIndex && tile.TileIndex <= ruleOriginalEndIndex)
                    {
                        if (rule.HeightOverride > -1)
                        {
                            byte height = (byte)Math.Min(rule.HeightOverride, 14);
                            if (tile.Level != height)
                            {
                                Logger.Debug("TileRules: Tile index " + tile.TileIndex + " at X: " + tile.X + ", Y:" + tile.Y + "  - height changed from " + tile.Level + " to " + height + ".");
                                tile.Level = height;
                            }
                        }
                        if (rule.SubIndexOverride > -1)
                        {
                            byte subtileIndex = (byte)Math.Min(rule.SubIndexOverride, 255);
                            if (tile.SubTileIndex != subtileIndex)
                            {
                                Logger.Debug("TileRules: Tile index " + tile.TileIndex + " at X: " + tile.X + ", Y:" + tile.Y + " - sub tile index changed from " + tile.SubTileIndex + " to " + subtileIndex + ".");
                                tile.SubTileIndex = subtileIndex;
                            }
                        }
                        if (rule.IceGrowthOverride > -1)
                        {
                            byte iceGrowth = Convert.ToByte(Convert.ToBoolean(rule.IceGrowthOverride));
                            if (tile.IceGrowth != iceGrowth)
                            {
                                Logger.Debug("TileRules: Tile index " + tile.TileIndex + " at X: " + tile.X + ", Y:" + tile.Y + " - ice growth flag changed from " + tile.IceGrowth + " to " + iceGrowth + ".");
                                tile.IceGrowth = iceGrowth;
                            }
                        }

                        if (rule.IsRandomizer)
                        {
                            int newindex = random.Next(ruleNewStartIndex, ruleNewEndIndex);
                            Logger.Debug("TileRules: Tile rule random range: [" + ruleNewStartIndex + "-" + ruleNewEndIndex + "]. Picked: " + newindex);
                            if (newindex != tile.TileIndex)
                            {
                                Logger.Debug("TileRules: Tile index " + tile.TileIndex + " at X: " + tile.X + ", Y:" + tile.Y + " - index changed to " + newindex);
                                tile.TileIndex = newindex;
                                tileDataAltered = true;
                            }
                            break;
                        }
                        else if (ruleNewEndIndex == ruleNewStartIndex)
                        {
                            Logger.Debug("TileRules: Tile index " + tile.TileIndex + " at X: " + tile.X + ", Y: " + tile.Y + " - index changed to " + ruleNewStartIndex);
                            tile.TileIndex = ruleNewStartIndex;
                            tileDataAltered = true;
                            break;
                        }
                        else
                        {
                            Logger.Debug("TileRules: Tile ID " + tile.TileIndex + " at X: " + tile.X + ", Y: " + tile.Y + " - index changed to " +
                                (ruleNewStartIndex + Math.Abs(ruleOriginalStartIndex - tile.TileIndex)));
                            tile.TileIndex = ruleNewStartIndex + Math.Abs(ruleOriginalStartIndex - tile.TileIndex);
                            tileDataAltered = true;
                            break;
                        }
                    }
                }
            }

            return tileDataAltered;
        }

        /// <summary>
        /// Removes level 0 clear tiles from IsoMapPack5 data.
        /// </summary>
        /// <returns>Returns true if tile data was changed, false if not.</returns>
        private bool RemoveLevel0ClearTiles()
        {
            if (!Initialized || !removeLevel0ClearTiles)
                return false;

            Logger.Info("RemoveLevel0ClearTiles set: All tile data with tile index & level set to 0 is removed.");

            List<MapTile> removeTiles = new List<MapTile>();

            foreach (MapTile tile in isoMapPack5)
            {
                if (tile.TileIndex < 1 && tile.Level < 1 && tile.SubTileIndex < 1 && tile.IceGrowth < 1)
                    removeTiles.Add(tile);
            }

            int removeCount = isoMapPack5.RemoveAll(x => removeTiles.Contains(x));

            return removeCount > 0;
        }

        /// <summary>
        /// Sorts tiles in map pack based on the set sorting method.
        /// </summary>
        /// <returns>Returns true if tile data was changed, false if not.</returns>
        private bool SortIsoMapPack()
        {
            if (!Initialized || isoMapPack5.Count < 1 || isoMapPack5SortBy == IsoMapPack5SortMode.NotDefined)
                return false;

            Logger.Info("IsoMapPack5SortBy set: IsoMapPack5 data will be sorted using sorting mode: " + isoMapPack5SortBy);
            switch (isoMapPack5SortBy)
            {
                case IsoMapPack5SortMode.XLevelTileIndex:
                    isoMapPack5 = isoMapPack5.OrderBy(x => x.X).ThenBy(x => x.Level).ThenBy(x => x.TileIndex).ToList();
                    break;
                case IsoMapPack5SortMode.XTileIndexLevel:
                    isoMapPack5 = isoMapPack5.OrderBy(x => x.X).ThenBy(x => x.TileIndex).ThenBy(x => x.Level).ToList();
                    break;
                case IsoMapPack5SortMode.TileIndexXLevel:
                    isoMapPack5 = isoMapPack5.OrderBy(x => x.TileIndex).ThenBy(x => x.X).ThenBy(x => x.Level).ToList();
                    break;
                case IsoMapPack5SortMode.LevelXTileIndex:
                    isoMapPack5 = isoMapPack5.OrderBy(x => x.Level).ThenBy(x => x.X).ThenBy(x => x.TileIndex).ToList();
                    break;
                case IsoMapPack5SortMode.X:
                    isoMapPack5 = isoMapPack5.OrderBy(x => x.X).ToList();
                    break;
                case IsoMapPack5SortMode.Level:
                    isoMapPack5 = isoMapPack5.OrderBy(x => x.Level).ToList();
                    break;
                case IsoMapPack5SortMode.TileIndex:
                    isoMapPack5 = isoMapPack5.OrderBy(x => x.TileIndex).ToList();
                    break;
                case IsoMapPack5SortMode.SubTileIndex:
                    isoMapPack5 = isoMapPack5.OrderBy(x => x.SubTileIndex).ToList();
                    break;
                case IsoMapPack5SortMode.IceGrowth:
                    isoMapPack5 = isoMapPack5.OrderBy(x => x.IceGrowth).ToList();
                    break;
                case IsoMapPack5SortMode.Y:
                    isoMapPack5 = isoMapPack5.OrderBy(x => x.Y).ToList();
                    break;
                default:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Changes overlay data of current map based on conversion profile.
        /// </summary>
        public void ConvertOverlayData()
        {
            if (!Initialized || overlayRules == null || overlayRules.Count < 1)
                return;

            else if (!IsCurrentTheaterAllowed())
            {
                Logger.Warn("Skipping altering overlay data - ApplicableTheaters does not contain entry matching map theater.");
                return;
            }

            ParseOverlayPacks();

            bool overlayDataAltered = ApplyOverlayConversionRules();

            if (overlayDataAltered)
                SaveOverlayPacks();

            MapAltered |= overlayDataAltered;
        }

        /// <summary>
        /// Processes overlay data conversion rules.
        /// </summary>
        /// <returns>Returns true if overlay data was changed, false if not.</returns>
        private bool ApplyOverlayConversionRules()
        {
            Logger.Info("Attempting to apply OverlayRules on map overlay data.");

            bool overlayDataChanged = false;

            for (int i = 0; i < OVERLAY_DATA_LENGTH; i++)
            {
                /*
                if (overlayPack[i] == 255)
                {
                    overlayDataPack[i] = 0;
                    continue;
                }*/
                if (overlayPack[i] < 0 || overlayPack[i] > 255)
                    overlayPack[i] = 255;
                if (overlayDataPack[i] < 0 || overlayDataPack[i] > 255)
                    overlayDataPack[i] = 0;

                int x = i % 512;
                int y = (i - x) / 512;

                foreach (OverlayConversionRule rule in overlayRules)
                {
                    if (!rule.IsValid)
                        continue;

                    if (rule.CoordinateFilterX > -1 && rule.CoordinateFilterX != x ||
                        rule.CoordinateFilterY > -1 && rule.CoordinateFilterY != y)
                        continue;

                    overlayDataChanged |= ChangeOverlayData(overlayPack, i, x, y, rule.OriginalStartIndex, rule.OriginalEndIndex,
                        rule.NewStartIndex, rule.NewEndIndex, rule.IsRandomizer, false);

                    overlayDataChanged |= ChangeOverlayData(overlayDataPack, i, x, y, rule.OriginalStartFrameIndex, rule.OriginalEndFrameIndex,
                        rule.NewStartFrameIndex, rule.NewEndFrameIndex, rule.IsFrameRandomizer, true);
                }
            }
            return overlayDataChanged;
        }

        /// <summary>
        /// Changes overlay data.
        /// </summary>
        /// <param name="data">Data collection to change.</param>
        /// <param name="index">Overlay index in the data collection.</param>
        /// <param name="x">Overlay X coordinate.</param>
        /// <param name="y">Overlay Y coordinate.</param>
        /// <param name="originalStartIndex">Original start index.</param>
        /// <param name="originalEndIndex">Original end index.</param>
        /// <param name="newStartIndex">New start index.</param>
        /// <param name="newEndIndex">New end index.</param>
        /// <param name="useRandomRange">If true, use a random range.</param>
        /// <param name="changeFrameData">If true, treat changes as being made to frame data rather than overlay ID data.</param>
        /// <returns>Returns true if overlay data was changed, false if not.</returns>
        private bool ChangeOverlayData(byte[] data, int index, int x, int y, int originalStartIndex, int originalEndIndex,
            int newStartIndex, int newEndIndex, bool useRandomRange, bool changeFrameData)
        {
            string dataType = changeFrameData ? "frame" : "ID";
            if (data[index] >= originalStartIndex && data[index] <= originalEndIndex)
            {
                if (useRandomRange)
                {
                    byte newindex = (byte)random.Next(newStartIndex, newEndIndex);
                    Logger.Debug("OverlayRules: Random " + dataType + " range [" + newStartIndex + "-" + newEndIndex + "]. Picked: " + newindex);
                    if (newindex != data[index])
                    {
                        Logger.Debug("OverlayRules: Overlay " + dataType + " " + data[index] + " at array slot " + index + " (X: " + x + ", Y: " + y + ") changed to " +
                            newindex + ".");
                        data[index] = newindex;
                        return true;
                    }
                }
                else if (newEndIndex == newStartIndex)
                {
                    Logger.Debug("OverlayRules: Overlay " + dataType + " " + data[index] + " at array slot " + index + " (X: " + x + ", Y: " + y + ") changed to " +
                        newStartIndex + ".");
                    data[index] = (byte)newStartIndex;
                    return true;
                }
                else
                {
                    Logger.Debug("OverlayRules: Overlay " + dataType + " " + data[index] + " at array slot " + index + " (X: " + x + ", Y: " + y + ") changed to " +
                        (newStartIndex + Math.Abs(originalStartIndex - data[index])) + ".");
                    data[index] = (byte)(newStartIndex + Math.Abs(originalStartIndex - data[index]));
                    return true;
                }
                return false;
            }
            else
                return false;
        }

        /// <summary>
        /// Changes object data of current map based on conversion profile.
        /// </summary>
        public void ConvertObjectData()
        {
            if (!Initialized || overlayRules == null || objectRules.Count < 1) return;
            else if (mapTheater != null && applicableTheaters != null && !applicableTheaters.Contains(mapTheater))
            {
                Logger.Warn("Conversion profile not applicable to maps belonging to this theater. No alterations will be made to the object data.");
                return;
            }
            Logger.Info("Attempting to modify object data of the map file.");
            ApplyObjectConversionRules("Aircraft");
            ApplyObjectConversionRules("Units");
            ApplyObjectConversionRules("Infantry");
            ApplyObjectConversionRules("Structures");
            ApplyObjectConversionRules("Terrain");
        }

        /// <summary>
        /// Processes object data conversion rules.
        /// </summary>
        /// <param name="sectionName">ID of the object list section to apply the rules to.</param>
        private void ApplyObjectConversionRules(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName)) return;
            KeyValuePair<string, string>[] kvps = mapINI.GetKeyValuePairs(sectionName);
            if (kvps == null) return;
            foreach (KeyValuePair<string, string> kvp in kvps)
            {
                foreach (StringConversionRule rule in objectRules)
                {
                    if (rule == null || rule.Original == null) continue;
                    if (CheckIfObjectIDMatches(kvp.Value, rule.Original))
                    {
                        if (rule.New == null)
                        {
                            Logger.Debug("ObjectRules: Removed " + sectionName + " object with ID '" + rule.Original + "' from the map file.");
                            mapINI.RemoveKey(sectionName, kvp.Key);
                            MapAltered = true;
                        }
                        else
                        {
                            Logger.Debug("ObjectRules: Replaced " + sectionName + " object with ID '" + rule.Original + "' with object of ID '" + rule.New + "'.");
                            mapINI.SetKey(sectionName, kvp.Key, kvp.Value.Replace(rule.Original, rule.New));
                            MapAltered = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes objects outside map bounds.
        /// </summary>
        private void DeleteObjectsOutsideBounds()
        {
            DeleteObjectsOutsideBoundsFromSection("Units");
            DeleteObjectsOutsideBoundsFromSection("Infantry");
            DeleteObjectsOutsideBoundsFromSection("Units");
            DeleteObjectsOutsideBoundsFromSection("Structures");

            string[] keys = mapINI.GetKeys("Terrain");
            if (keys == null)
                return;

            foreach (string key in keys)
            {
                int coord = Conversion.GetIntFromString(key, -1);
                if (coord < 0)
                    continue;
                int x = coord % 1000;
                int y = (coord - x) / 1000;

                if (!CoordinateExistsOnMap(x, y))
                {
                    Logger.Debug("DeleteObjectsOutsideMapBounds: Removed Terrain object " + mapINI.GetKey("Terrain", key, "") +
                        " (key: " + key + ") from cell " + x + "," + y + ".");
                    mapINI.RemoveKey("Terrain", key);
                    MapAltered = true;
                }
            }
        }

        /// <summary>
        /// Deletes specific types of objects outside map bounds.
        /// </summary>
        /// <param name="sectionName"></param>
        private void DeleteObjectsOutsideBoundsFromSection(string sectionName)
        {
            string[] keys = mapINI.GetKeys(sectionName);
            if (keys == null) return;
            foreach (string key in keys)
            {
                string[] tmp = mapINI.GetKey(sectionName, key, "").Split(',');
                if (tmp.Length < 5)
                    continue;
                int x = Conversion.GetIntFromString(tmp[3], -1);
                int y = Conversion.GetIntFromString(tmp[4], -1);
                if (x < 0 || y < 0)
                    continue;
                if (!CoordinateExistsOnMap(x, y))
                {
                    string[] values = mapINI.GetKey(sectionName, key, "").Split(',');
                    Logger.Debug("DeleteObjectsOutsideMapBounds: Removed " + sectionName + " object " + (values.Length > 1 ? values[1] : "???") +
                        " (key: " + key + ") from cell " + x + "," + y + ".");
                    mapINI.RemoveKey(sectionName, key);
                    MapAltered = true;
                }
            }
        }

        /// <summary>
        /// Deletes overlays outside map bounds.
        /// </summary>
        private void DeleteOverlaysOutsideBounds()
        {
            if (overlayPack == null || overlayDataPack == null)
            {
                ParseOverlayPacks();
            }

            if (overlayPack == null || overlayDataPack == null)
                return;

            bool overlayDataAltered = false;

            for (int i = 0; i < overlayPack.Length; i++)
            {
                if (overlayPack[i] == 255)
                    continue;
                int x = i % 512;
                int y = (i - x) / 512;

                if (!CoordinateExistsOnMap(x, y))
                {
                    Logger.Debug("DeleteObjectsOutsideMapBounds: Removed overlay (index: " + overlayPack[i] + ") from cell " + x + "," + y + ".");
                    overlayPack[i] = 255;
                    overlayDataPack[i] = 0;
                    overlayDataAltered = true;
                }
            }

            if (overlayDataAltered)
                SaveOverlayPacks();

            MapAltered |= overlayDataAltered;
        }

        /// <summary>
        /// Fixes tunnels.
        /// Based on Rampastring's FinalSun Tunnel Fixer.
        /// https://ppmforums.com/viewtopic.php?t=42008
        /// </summary>
        private void FixTubesSection()
        {
            string[] keys = mapINI.GetKeys("Tubes");

            if (keys == null)
                return;

            int counter = 0;
            foreach (string key in keys)
            {
                List<string> values = mapINI.GetKey("Tubes", key, string.Empty).Split(',').ToList();

                int index = values.FindIndex(str => str == "-1");

                if (index < 1 || index > values.Count - 3)
                    continue;

                if (counter % 2 == 0)
                {
                    Logger.Debug("FixTunnels: Set -1 at index " + index + " in tube #" + counter + " to " + values[index - 1] + ".");
                    values[index] = values[index - 1];
                    values.RemoveRange(index + 2, values.Count - (index + 2));
                }
                else
                    values.RemoveRange(index + 1, values.Count - (index + 1));
                mapINI.SetKey("Tubes", key, string.Join(",", values));
                MapAltered = true;
                counter++;
            }
        }

        /// <summary>
        /// Changes section data of current map based on conversion profile.
        /// </summary>
        public void ConvertSectionData()
        {
            if (!Initialized || sectionRules == null || sectionRules.Count < 1) return;
            else if (!IsCurrentTheaterAllowed())
            {
                Logger.Warn("Skipping altering section data - ApplicableTheaters does not contain entry matching map theater.");
                return;
            }
            Logger.Info("Attempting to modify section data of the map file.");
            ApplySectionConversionRules();
        }

        /// <summary>
        /// Processes section data conversion rules.
        /// </summary>
        private void ApplySectionConversionRules()
        {
            foreach (SectionConversionRule rule in sectionRules)
            {
                if (string.IsNullOrEmpty(rule.OriginalSection))
                    continue;

                string currentSection = rule.OriginalSection;
                if (rule.NewSection == null)
                {
                    Logger.Debug("SectionRules: Removed section '" + rule.OriginalSection + "'.");
                    mapINI.RemoveSection(rule.OriginalSection);
                    MapAltered = true;
                    continue;
                }
                else if (rule.NewSection != "")
                {
                    if (!mapINI.SectionExists(rule.OriginalSection))
                    {
                        Logger.Debug("SectionRules: Added new section '" + rule.NewSection + "'.");
                        mapINI.AddSection(rule.NewSection);
                    }
                    else
                    {
                        Logger.Debug("SectionRules: Renamed section '" + rule.OriginalSection + "' to '" + rule.NewSection + "'.");
                        mapINI.RenameSection(rule.OriginalSection, rule.NewSection);
                    }
                    MapAltered = true;
                    currentSection = rule.NewSection;
                }

                string currentKey = rule.OriginalKey;
                if (rule.NewKey == null)
                {
                    Logger.Debug("SectionRules: Removed key '" + rule.OriginalKey + "' from section '" + currentSection + "'.");
                    mapINI.RemoveKey(currentSection, rule.OriginalKey);
                    MapAltered = true;
                    continue;
                }
                else if (rule.NewKey != "")
                {
                    if (mapINI.GetKey(currentSection, rule.OriginalKey, null) == null)
                    {
                        Logger.Debug("SectionRules: Added a new key '" + rule.NewKey + "' to section '" + currentSection + "'.");
                        mapINI.SetKey(currentSection, rule.NewKey, "");
                    }
                    else
                    {
                        Logger.Debug("SectionRules: Renamed key '" + rule.OriginalKey + "' in section '" + currentSection + "' to '" + rule.NewKey + "'.");
                        mapINI.RenameKey(currentSection, rule.OriginalKey, rule.NewKey);
                    }
                    MapAltered = true;
                    currentKey = rule.NewKey;
                }

                if (rule.NewValue != "")
                {
                    Logger.Debug("SectionRules: Section '" + currentSection + "' key '" + currentKey + "' value changed to '" + rule.NewValue + "'.");
                    mapINI.SetKey(currentSection, currentKey, rule.NewValue);
                    MapAltered = true;
                }
            }
        }

        #endregion

        #region helpers

        /// <summary>
        /// Checks if map object declaration matches with specific object ID.
        /// </summary>
        /// <param name="objectDeclaration">Object declaration from map file.</param>
        /// <param name="objectID">Object ID.</param>
        /// <returns>True if a match, otherwise false.</returns>
        private bool CheckIfObjectIDMatches(string objectDeclaration, string objectID)
        {
            if (objectDeclaration.Equals(objectID)) return true;
            string[] sp = objectDeclaration.Split(',');
            if (sp.Length < 2) return false;
            if (sp[1].Equals(objectID)) return true;
            return false;
        }

        /// <summary>
        /// Checks if location with given coordinates exists within map bounds.
        /// </summary>
        /// <param name="x">Location X coordinate.</param>
        /// <param name="y">Location Y coordinate.</param>
        /// <returns>True if location exists, false if not.</returns>
        private bool CoordinateExistsOnMap(int x, int y)
        {
            if (!Initialized || coordinateValidityLUT == null ||
                coordinateValidityLUT.GetLength(0) <= x || coordinateValidityLUT.GetLength(1) <= y)
                return false;

            return coordinateValidityLUT[x, y];
        }

        /// <summary>
        /// Calculates valid map coordinates from map width & height and creates a look-up table from them.
        /// </summary>
        private void CalculateCoordinateValidity()
        {
            Logger.Debug("Calculating map coordinate look-up table.");

            int size = Math.Max(mapWidth, mapHeight) * 2 + 1;
            coordinateValidityLUT = new bool[size, size];

            int yOffset = 0;
            for (int col = 1; col <= mapWidth; col++)
            {
                int startY = mapWidth - yOffset;
                for (int row = 0; row < mapHeight; row++)
                {
                    int x = col + row;
                    int y = startY + row;
                    coordinateValidityLUT[x, y] = true;
                    if (col < mapWidth)
                        coordinateValidityLUT[x + 1, y] = true;
                }
                yOffset += 1;
            }
        }

        /// <summary>
        /// Merges array of string key-value pairs to a single string array containing strings of the keys and values separated by =.
        /// </summary>
        /// <param name="keyValuePairs">Array of string key-value pairs.</param>
        /// <returns>Array of strings made by merging the keys and values.</returns>
        private string[] MergeKeyValuePairs(KeyValuePair<string, string>[] keyValuePairs)
        {
            if (keyValuePairs == null)
                return null;

            string[] result = new string[keyValuePairs.Length];
            for (int i = 0; i < keyValuePairs.Length; i++)
            {
                result[i] = keyValuePairs[i].Key + "=" + keyValuePairs[i].Value;
            }
            return result;
        }

        #endregion

        /// <summary>
        /// Lists theater config file data to a text file.
        /// </summary>
        public void ListTileSetData()
        {
            if (!Initialized || theaterConfigINI == null)
                return;

            List<Tileset> tilesets = ParseTilesetData();

            if (tilesets == null || tilesets.Count < 1)
            {
                Logger.Error("Could not parse tileset data from theater configuration file '" +
                    theaterConfigINI.Filename + "'."); return;
            };

            Logger.Info("Attempting to list tileset data for a theater based on file: '" + theaterConfigINI.Filename + "'.");
            List<string> lines = new List<string>();
            int tilecounter = 0;
            lines.Add("Theater tileset data gathered from file '" + theaterConfigINI.Filename + "'.");
            lines.Add("");
            lines.Add("");
            foreach (Tileset tileset in tilesets)
            {
                if (tileset.TilesInSet < 1)
                {
                    Logger.Debug("ListTileSetData: " + tileset.SetID + " (" + tileset.SetName + ")" + " skipped due to tile count of 0.");
                    continue;
                }
                lines.AddRange(tileset.GetPrintableData(tilecounter));
                lines.Add("");
                tilecounter += tileset.TilesInSet;
                Logger.Debug("ListTileSetData: " + tileset.SetID + " (" + tileset.SetName + ")" + " added to the list.");
            }
            File.WriteAllLines(FilenameOutput, lines.ToArray());
        }

        /// <summary>
        /// Parse tileset data from a theater configuration INI file.
        /// </summary>
        /// <returns>List of tilesets.</returns>
        private List<Tileset> ParseTilesetData()
        {
            List<Tileset> tilesets = new List<Tileset>();

            if (!Initialized || theaterConfigINI == null)
                return tilesets;

            string[] sections = theaterConfigINI.GetSections();
            foreach (string section in sections)
            {
                if (!section.StartsWith("TileSet"))
                    continue;

                Tileset tileset = new Tileset
                {
                    SetID = section,
                    SetNumber = Conversion.GetIntFromString(section.Substring(7, 4), -1),
                    SetName = theaterConfigINI.GetKey(section, "SetName", "N/A"),
                    FileName = theaterConfigINI.GetKey(section, "FileName", "N/A").ToLower(),
                    TilesInSet = Conversion.GetIntFromString(theaterConfigINI.GetKey(section, "TilesInSet", "0"), 0)
                };

                if (tileset.SetNumber == -1)
                    continue;

                tilesets.Add(tileset);
            }

            return tilesets;
        }
    }
}
