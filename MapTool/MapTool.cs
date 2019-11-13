/*
 * Copyright 2017 by Starkku
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
using CNCMaps.FileFormats.Encodings;
using CNCMaps.FileFormats.VirtualFileSystem;
using StarkkuUtils.FileTypes;
using StarkkuUtils.Utilities;
using System.Text.RegularExpressions;
using System.Globalization;

namespace MapTool
{
    /// <summary>
    /// Map file modifier tool.
    /// </summary>
    class MapTool
    {

        /// <summary>
        /// Has tool been initialized or not.
        /// </summary>
        public bool Initialized { get; set; }

        /// <summary>
        /// Has map file been altered or not.
        /// </summary>
        public bool Altered { get; set; }

        /// <summary>
        /// Map input filename.
        /// </summary>
        private readonly string filenameInput;
        /// <summary>
        /// Map output filename.
        /// </summary>
        private readonly string filenameOutput;

        /// <summary>
        /// Map file.
        /// </summary>
        private INIFile mapINI;
        /// <summary>
        /// Map theater.
        /// </summary>
        private string mapTheater = null;
        /// <summary>
        /// Map local width.
        /// </summary>
        private int mapLocalWidth;
        /// <summary>
        /// Map local height.
        /// </summary>
        private int mapLocalHeight;
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
        private List<MapTileContainer> isoMapPack5 = new List<MapTileContainer>();
        /// <summary>
        /// Map overlay ID data.
        /// </summary>
        private byte[] overlayPack = null;
        /// <summary>
        /// Map overlay frame data.
        /// </summary>
        private byte[] overlayDataPack = null;

        /// <summary>
        /// Conversion profile INI file.
        /// </summary>
        private INIFile conversionProfileINI;
        /// <summary>
        /// Conversion profile applicable theaters.
        /// </summary>
        private List<string> applicableTheaters = new List<string>();
        /// <summary>
        /// Conversion profile theater-specific global tile offsets.
        /// </summary>
        private Dictionary<string, Tuple<int, int>> theaterTileOffsets = new Dictionary<string, Tuple<int, int>>();
        /// <summary>
        /// Conversion profile new theater.
        /// </summary>
        private string newTheater = null;
        /// <summary>
        /// Conversion profile tile rules.
        /// </summary>
        private List<ByteIDConversionRule> tileRules = new List<ByteIDConversionRule>();
        /// <summary>
        /// Conversion profile overlay rules.
        /// </summary>
        private List<ByteIDConversionRule> overlayRules = new List<ByteIDConversionRule>();
        /// <summary>
        /// Conversion profile object rules.
        /// </summary>
        private List<StringIDConversionRule> objectRules = new List<StringIDConversionRule>();
        /// <summary>
        /// // Conversion profile section rules.
        /// </summary>
        private List<SectionConversionRule> sectionRules = new List<SectionConversionRule>();

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
        /// Building ID used to determine which tiles should have ice growth fix applied on them.
        /// </summary>
        private readonly string iceGrowthFixUseBuilding = null;
        /// <summary>
        /// Reset ice growth data on all tiles or not.
        /// </summary>
        private readonly bool iceGrowthFixReset = false;
        /// <summary>
        /// Fix tunnel data or not.
        /// </summary>
        private readonly bool fixTunnels = false;
        /// <summary>
        /// Map tile data sort mode.
        /// </summary>
        private IsoMapPack5SortMode isoMapPack5SortBy = IsoMapPack5SortMode.NotDefined;

        /// <summary>
        /// Theater configuration file.
        /// </summary>
        private INIFile theaterConfigINI;

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
            Altered = false;
            filenameInput = inputFile;
            filenameOutput = outputFile;

            if (listTheaterData && !string.IsNullOrEmpty(filenameInput))
            {
                theaterConfigINI = new INIFile(filenameInput);
                if (!theaterConfigINI.Initialized)
                {
                    Initialized = false;
                    return;
                }
            }

            else if (!string.IsNullOrEmpty(filenameInput) && !string.IsNullOrEmpty(filenameOutput))
            {

                Logger.Info("Reading map file '" + inputFile + "'.");
                mapINI = new INIFile(inputFile);
                if (!mapINI.Initialized)
                {
                    Initialized = false;
                    return;
                }
                string[] size = mapINI.GetKey("Map", "Size", "").Split(',');
                mapWidth = int.Parse(size[2]);
                mapHeight = int.Parse(size[3]);
                Initialized = ParseMapPack();
                mapTheater = mapINI.GetKey("Map", "Theater", null);
                if (mapTheater != null) mapTheater = mapTheater.ToUpper();

                conversionProfileINI = new INIFile(fileConfig);
                if (!conversionProfileINI.Initialized) Initialized = false;
                else
                {
                    Logger.Info("Parsing conversion profile file.");

                    string include_string = conversionProfileINI.GetKey("ProfileData", "IncludeFiles", null);
                    if (!string.IsNullOrEmpty(include_string))
                    {
                        string[] include_files = include_string.Split(',');
                        string basedir = Path.GetDirectoryName(fileConfig);
                        foreach (string f in include_files)
                        {
                            if (File.Exists(basedir + "\\" + f))
                            {
                                INIFile ic = new INIFile(basedir + "\\" + f);
                                if (!ic.Initialized) continue;
                                Logger.Info("Merging included file '" + f + "' to conversion profile.");
                                conversionProfileINI.Merge(ic);
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
                    iceGrowthFixUseBuilding = conversionProfileINI.GetKey("IsoMapPack5", "IceGrowthFixUseBuilding", null);
                    iceGrowthFixReset = Conversion.GetBoolFromString(conversionProfileINI.GetKey("IsoMapPack5", "IceGrowthFixReset", "false"), false);

                    // Parse theater rules.
                    newTheater = conversionProfileINI.GetKey("TheaterRules", "NewTheater", null);
                    if (newTheater != null)
                        newTheater = newTheater.ToUpper();

                    string[] applicableTheaters = null;
                    applicableTheaters = conversionProfileINI.GetKey("TheaterRules", "ApplicableTheaters", "").Split(',');
                    if (applicableTheaters != null)
                    {
                        for (int i = 0; i < applicableTheaters.Length; i++)
                        {
                            string theater = applicableTheaters[i].Trim().ToUpper();
                            if (theater == "") continue;
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
                            int originalOffset = 0;
                            int newOffset = int.MinValue;
                            string[] values = conversionProfileINI.GetKey("TheaterTileOffsets", key, "").Split(',');
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

                    // Allow saving map without any other changes if either of these are set and ApplicableTheaters allows it.
                    if ((useMapCompress || useMapOptimize || deleteObjectsOutsideMapBounds || fixTunnels) && IsCurrentTheaterAllowed())
                        Altered = true;

                    // Parse conversion rules.
                    string[] tilerules = null;
                    string[] overlayrules = null;
                    string[] objectrules = null;
                    string[] sectionrules = null;

                    tilerules = conversionProfileINI.GetValues("TileRules");
                    overlayrules = conversionProfileINI.GetValues("OverlayRules");
                    objectrules = conversionProfileINI.GetValues("ObjectRules");
                    sectionrules = MergeKeyValuePairs(conversionProfileINI.GetKeyValuePairs("SectionRules"));

                    if (!Altered && tilerules == null && overlayrules == null && objectrules == null && sectionrules == null &&
                        string.IsNullOrEmpty(newTheater))
                    {
                        Logger.Error("No conversion rules to apply in the conversion profile file. Aborting.");
                        Initialized = false;
                        return;
                    }

                    ParseConversionRules(tilerules, tileRules);
                    ParseConversionRules(overlayrules, overlayRules);
                    ParseConfigFile(objectrules, objectRules);
                    ParseConfigFile(sectionrules, sectionRules);
                }
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
                Logger.Info("DeleteObjectsOutsideMapBounds set: Deleting objects & overlay outside map bounds.");
                DeleteOverlaysOutsideBounds();
                DeleteObjectsOutsideBounds();
            }
            if (useMapOptimize)
            {
                Logger.Info("ApplyMapOptimization set: Saved map will have map section order optimizations applied.");
                mapINI.MoveSectionToFirst("Basic");
                mapINI.MoveSectionToFirst("MultiplayerDialogSettings");
                mapINI.MoveSectionToLast("Digest");
            }
            if (fixTunnels)
            {
                Logger.Info("FixTunnels set: Saved map will have [Tubes] section fixed to remove errors caused by map editor.");
                FixTubesSection();
            }
            if (useMapCompress)
                Logger.Info("ApplyMapCompress set: Saved map will have no unnecessary whitespaces or comments.");
            mapINI.Save(filenameOutput, !useMapCompress, !useMapCompress);
        }

        /// <summary>
        /// Checks if theater name is valid.
        /// </summary>
        /// <param name="theaterName">Theater name.</param>
        /// <returns>True if a valid theater name, otherwise false.</returns>
        public static bool IsValidTheaterName(string theaterName)
        {
            if (theaterName == "TEMPERATE" || theaterName == "SNOW" || theaterName == "LUNAR" || theaterName == "DESERT" ||
                theaterName == "URBAN" || theaterName == "NEWURBAN") return true;
            return false;
        }

        /// <summary>
        /// Checks if the currently set map theater exists in current list of theaters the map tool is allowed to process.
        /// </summary>
        /// <returns>True if map theater exists in applicable theaters, otherwise false.</returns>
        private bool IsCurrentTheaterAllowed()
        {
            if (applicableTheaters == null || mapTheater == null || !applicableTheaters.Contains(mapTheater)) return false;
            return true;
        }

        /// <summary>
        /// Parses IsoMapPack5 section of the map file.
        /// </summary>
        /// <returns>True if success, otherwise false.</returns>
        private bool ParseMapPack()
        {
            Logger.Info("Parsing IsoMapPack5.");
            string data = "";
            string[] tmp = mapINI.GetValues("IsoMapPack5");
            if (tmp == null || tmp.Length < 1) return false;
            data = string.Join("", tmp);
            int cells;
            byte[] isoMapPack;
            try
            {
                string size = mapINI.GetKey("Map", "Size", "");
                string[] st = size.Split(',');
                mapLocalWidth = Convert.ToInt16(st[2]);
                mapLocalHeight = Convert.ToInt16(st[3]);
                byte[] lzoData = Convert.FromBase64String(data);
                byte[] test = lzoData;
                cells = (mapLocalWidth * 2 - 1) * mapLocalHeight;
                int lzoPackSize = cells * 11 + 4;
                isoMapPack = new byte[lzoPackSize];
                // Fill up and filter later
                int j = 0;
                for (int i = 0; i < cells; i++)
                {
                    isoMapPack[j] = 0x88;
                    isoMapPack[j + 1] = 0x40;
                    isoMapPack[j + 2] = 0x88;
                    isoMapPack[j + 3] = 0x40;
                    j += 11;
                }
                uint total_decompress_size = Format5.DecodeInto(lzoData, isoMapPack);
            }
            catch (Exception)
            {
                return false;
            }
            MemoryFile mf = new MemoryFile(isoMapPack);
            for (int i = 0; i < cells; i++)
            {
                ushort x = mf.ReadUInt16();
                ushort y = mf.ReadUInt16();
                int tileNum = mf.ReadInt32();
                byte subTile = mf.ReadByte();
                byte level = mf.ReadByte();
                byte iceGrowth = mf.ReadByte();
                int dx = x - y + mapWidth - 1;
                int dy = x + y - mapWidth - 1;
                if (x > 0 && y > 0 && x <= 16384 && y <= 16384)
                {
                    isoMapPack5.Add(new MapTileContainer((short)x, (short)y, tileNum, subTile, level, iceGrowth));
                }
            }
            return true;
        }

        /// <summary>
        /// Parses Overlay(Data)Pack section(s) of the map file.
        /// </summary>
        private void ParseOverlayPack()
        {
            Logger.Info("Parsing OverlayPack.");
            string[] values = mapINI.GetValues("OverlayPack");
            if (values == null || values.Length < 1) return;
            byte[] format80Data = Convert.FromBase64String(string.Join("", values));
            var overlaypack = new byte[1 << 18];
            Format5.DecodeInto(format80Data, overlaypack, 80);

            Logger.Info("Parsing OverlayDataPack.");
            values = mapINI.GetValues("OverlayDataPack");
            if (values == null || values.Length < 1) return;
            format80Data = Convert.FromBase64String(string.Join("", values));
            var overlaydatapack = new byte[1 << 18];
            Format5.DecodeInto(format80Data, overlaydatapack, 80);

            overlayPack = overlaypack;
            overlayDataPack = overlaydatapack;
        }

        /// <summary>
        /// Saves Overlay(Data)Pack section(s) of the map file.
        /// </summary>
        private void SaveOverlayPack()
        {
            string base64_overlayPack = Convert.ToBase64String(Format5.Encode(overlayPack, 80), Base64FormattingOptions.None);
            string base64_overlayDataPack = Convert.ToBase64String(Format5.Encode(overlayDataPack, 80), Base64FormattingOptions.None);
            OverrideBase64MapSection("OverlayPack", base64_overlayPack);
            OverrideBase64MapSection("OverlayDataPack", base64_overlayDataPack);
            Altered = true;
        }

        /// <summary>
        /// Replaces contents of a base64-encoded section of map file.
        /// </summary>
        /// <param name="sectionName">Name of the section to replace.</param>
        /// <param name="data">Contents to replace the existing contents with.</param>
        private void OverrideBase64MapSection(string sectionName, string data)
        {
            int lx = 70;
            List<string> lines = new List<string>();
            for (int x = 0; x < data.Length; x += lx)
            {
                lines.Add(data.Substring(x, Math.Min(lx, data.Length - x)));
            }
            mapINI.ReplaceSectionKeysAndValues(sectionName, lines);
        }

        /// <summary>
        /// Parses conversion profile information for byte ID-type rules.
        /// </summary>
        private void ParseConversionRules(string[] newRules, List<ByteIDConversionRule> currentRules)
        {
            if (newRules == null || newRules.Length < 1 || currentRules == null) return;
            currentRules.Clear();
            bool value1IsARange = false;
            bool value2IsARange = false;
            bool isRandomizer = false;
            int value1Part1 = 0;
            int value1Part2 = 0;
            int value2Part1 = 0;
            int value2Part2 = 0;

            foreach (string str in newRules)
            {
                string[] values = str.Split('|');
                if (values.Length < 2) continue;

                if (values[0].Contains('-'))
                {
                    value1IsARange = true;
                    string[] values_1 = values[0].Split('-');
                    value1Part1 = Conversion.GetIntFromString(values_1[0], -1);
                    value1Part2 = Conversion.GetIntFromString(values_1[1], -1);
                    if (value1Part1 < 0 || value1Part2 < 0)
                        continue;
                }
                else
                {
                    value1Part1 = Conversion.GetIntFromString(values[0], -1);
                    if (value1Part1 < 0)
                        continue;
                }

                if (values[1].Contains('-'))
                {
                    value2IsARange = true;
                    string[] values_2 = values[1].Split('-');
                    value2Part1 = Conversion.GetIntFromString(values_2[0], -1);
                    value2Part2 = Conversion.GetIntFromString(values_2[1], -1);
                    if (value2Part1 < 0 || value2Part2 < 0)
                        continue;
                }
                else if (values[1].Contains('~'))
                {
                    value2IsARange = true;
                    string[] values_2 = values[1].Split('~');
                    value2Part1 = Conversion.GetIntFromString(values_2[0], -1);
                    value2Part2 = Conversion.GetIntFromString(values_2[1], -1);
                    if (value2Part1 < 0 || value2Part2 < 0 || value2Part1 >= value2Part2)
                        continue;
                    isRandomizer = true;
                }
                else
                {
                    value2Part1 = Conversion.GetIntFromString(values[1], -1);
                    if (value2Part1 < 0)
                        continue;
                }

                int heightOverride = -1;
                int subTileOverride = -1;
                if (values.Length >= 3 && values[2] != null && !values[2].Equals("*", StringComparison.InvariantCultureIgnoreCase))
                {
                    heightOverride = Conversion.GetIntFromString(values[2], -1);
                }
                if (values.Length >= 4 && values[3] != null && !values[3].Equals("*", StringComparison.InvariantCultureIgnoreCase))
                {
                    subTileOverride = Conversion.GetIntFromString(values[3], -1);
                }

                if ((value1IsARange && value2IsARange))
                {
                    currentRules.Add(new ByteIDConversionRule(value1Part1, value2Part1, value1Part2, value2Part2, heightOverride, subTileOverride, isRandomizer));
                }
                else if (value1IsARange && !value2IsARange)
                {
                    int diff = value2Part1 + (value1Part2 - value1Part1);
                    currentRules.Add(new ByteIDConversionRule(value1Part1, value2Part1, value1Part2, diff, heightOverride, subTileOverride, isRandomizer));
                }
                else if (!value1IsARange && value2IsARange)
                {
                    currentRules.Add(new ByteIDConversionRule(value1Part1, value2Part1, value1Part1, value2Part2, heightOverride, subTileOverride, isRandomizer));
                }
                else
                {
                    currentRules.Add(new ByteIDConversionRule(value1Part1, value2Part1, -1, -1, heightOverride, subTileOverride, isRandomizer));
                }
                value1IsARange = false;
                value2IsARange = false;
                isRandomizer = false;
            }
        }

        /// <summary>
        /// Parses conversion profile information for string ID-type rules.
        /// </summary>
        private void ParseConfigFile(string[] new_rules, List<StringIDConversionRule> current_rules)
        {
            if (new_rules == null || new_rules.Length < 1 || current_rules == null) return;
            current_rules.Clear();
            foreach (string str in new_rules)
            {
                string[] values = str.Split('|');
                if (values.Length == 1) current_rules.Add(new StringIDConversionRule(values[0], null));
                else if (values.Length >= 2) current_rules.Add(new StringIDConversionRule(values[0], values[1]));
            }
        }

        /// <summary>
        /// Parses conversion profile information for map file section rules.
        /// </summary>
        private void ParseConfigFile(string[] newRules, List<SectionConversionRule> currentRules)
        {
            if (newRules == null || newRules.Length < 1 || currentRules == null) return;
            currentRules.Clear();
            foreach (string str in newRules)
            {
                if (str == null || str.Length < 1) continue;
                string[] values = str.Split('|');
                string originalSection = "";
                string newSection = "";
                string originalKey = "";
                string newKey = "";
                string newValue = "";
                if (values.Length > 0)
                {
                    if (values[0].StartsWith("=")) values[0] = values[0].Substring(1, values[0].Length - 1);
                    string[] sec = values[0].Split('=');
                    if (sec == null || sec.Length < 1) continue;
                    originalSection = sec[0];
                    if (sec.Length == 1 && values[0].Contains('=') || sec.Length > 1 && values[0].Contains('=') &&
                        string.IsNullOrEmpty(sec[1])) newSection = null;
                    else if (sec.Length > 1) newSection = sec[1];
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
        /// Changes theater declaration of current map based on conversion profile.
        /// </summary>
        public void ConvertTheaterData()
        {
            if (!Initialized || string.IsNullOrEmpty(newTheater)) return;
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
                Altered = true;
            }
        }

        /// <summary>
        /// Changes tile data of current map based on conversion profile.
        /// </summary>
        public void ConvertTileData()
        {
            if (!Initialized || isoMapPack5.Count < 1 || tileRules == null || tileRules.Count < 1)
                return;
            else if (!IsCurrentTheaterAllowed())
            {
                Logger.Warn("Skipping altering tile data - ApplicableTheaters does not contain entry matching map theater.");
                return;
            }
            Logger.Info("Attempting to modify tile data of the map file.");
            ApplyTileConversionRules();
        }

        /// <summary>
        /// Processes tile data conversion rules.
        /// </summary>
        private void ApplyTileConversionRules()
        {
            List<MapTileContainer> tilesetForSort = new List<MapTileContainer>();
            List<MapTileContainer> tilesetSorted = new List<MapTileContainer>();
            List<Tuple<short, short>> tilesXY = new List<Tuple<short, short>>();
            Random random = new Random();
            // Apply tile conversion rules
            int originalOffset = 0, newOffset = 0, ruleOriginalStartIndex = 0, ruleOriginalEndIndex = 0, ruleNewStartIndex = 0, ruleNewEndIndex = 0;
            if (theaterTileOffsets.ContainsKey(mapTheater.ToUpper()))
            {
                originalOffset = theaterTileOffsets[mapTheater.ToUpper()].Item1;
                newOffset = theaterTileOffsets[mapTheater.ToUpper()].Item2;
                if (newOffset == int.MinValue)
                    newOffset = originalOffset;
                if (originalOffset != 0 && newOffset != 0)
                    Logger.Info("Global tile rule offsets for theater " + mapTheater.ToUpper() + ": " + originalOffset + " (original), " + newOffset + " (new)");
            }
            foreach (MapTileContainer tile in isoMapPack5)
            {
                if (tile.TileIndex < 0 || tile.TileIndex == 65535)
                    tile.TileIndex = 0;
                foreach (ByteIDConversionRule rule in tileRules)
                {
                    ruleOriginalStartIndex = rule.OriginalStartIndex + originalOffset;
                    ruleOriginalEndIndex = rule.OriginalEndIndex + originalOffset;
                    ruleNewStartIndex = rule.NewStartIndex + newOffset;
                    ruleNewEndIndex = rule.NewEndIndex + newOffset;

                    if (tile.TileIndex >= ruleOriginalStartIndex && tile.TileIndex <= ruleOriginalEndIndex)
                    {
                        if (rule.HeightOverride >= 0)
                        {
                            tile.Level = (byte)Math.Min(rule.HeightOverride, 14);
                        }
                        if (rule.SubIndexOverride >= 0)
                        {
                            tile.SubTileIndex = (byte)Math.Min(rule.SubIndexOverride, 255);
                        }
                        if (rule.IsRandomizer)
                        {
                            int newindex = random.Next(ruleNewStartIndex, ruleNewEndIndex);
                            Logger.Debug("TileRules: Tile rule random range: [" + ruleNewStartIndex + "-" + ruleNewEndIndex + "]. Picked: " + newindex);
                            if (newindex != tile.TileIndex)
                            {
                                Logger.Debug("TileRules: Tile ID " + tile.TileIndex + " at " + tile.X + "," + tile.Y + " changed to " + newindex);
                                tile.TileIndex = newindex;
                            }
                            break;
                        }
                        else if (ruleNewEndIndex == ruleNewStartIndex)
                        {
                            Logger.Debug("TileRules: Tile ID " + tile.TileIndex + " at " + tile.X + "," + tile.Y + " changed to " + ruleNewStartIndex);
                            tile.TileIndex = ruleNewStartIndex;
                            break;
                        }
                        else
                        {
                            Logger.Debug("TileRules: Tile ID " + tile.TileIndex + " at " + tile.X + "," + tile.Y + " changed to " +
                                (ruleNewStartIndex + Math.Abs(ruleOriginalStartIndex - tile.TileIndex)));
                            tile.TileIndex = ruleNewStartIndex + Math.Abs(ruleOriginalStartIndex - tile.TileIndex);
                            break;
                        }
                    }
                }
            }

            // Fix for TS Snow Maps Ice Growth, FinalSun sets all IceGrowth byte to 0
            // Using a defined building to get a list of X, Y then to set IceGrowth to 1
            string[] buildings = mapINI.GetValues("Structures");
            if (iceGrowthFixUseBuilding != null && buildings != null && buildings.Length > 0)
            {
                foreach (string building in buildings)
                {
                    string[] values = building.Split(',');
                    if (values != null && values.Length > 1)
                    {
                        string buildingID = values[1].Trim();
                        if (buildingID != "" && buildingID == iceGrowthFixUseBuilding)
                        {
                            short x = Conversion.GetShortFromString(values[3], -1);
                            short y = Conversion.GetShortFromString(values[4], -1);
                            if (x == -1 || y == -1)
                                continue;
                            tilesXY.Add(new Tuple<short, short>(x, y));
                        }
                    }
                }
            }
            if (iceGrowthFixReset)
                Logger.Info("IceGrowthFixReset set: Will attempt to disable ice growth for entire map.");
            else if (tilesXY.Count > 0)
                Logger.Info("IceGrowthFixUseBuilding set: Will attempt to enable ice growth for tiles with coordinates from building ID: " +
                    iceGrowthFixUseBuilding);
            else if (iceGrowthFixUseBuilding != null && tilesXY.Count < 1)
                Logger.Warn("IceGrowthFixUseBuilding is set but no instances of the building were found on the map.");

            if (removeLevel0ClearTiles)
                Logger.Info("RemoveLevel0ClearTiles set: Will attempt to remove all tile data with tile index & level set to 0");

            // Remove Height Level 0 Clear Tiles if set in profile
            foreach (MapTileContainer t in isoMapPack5)
            {
                // Set IceGrowth byte to 1 for Ice Growth for specific tiles. If Reset, set all to 0
                if (tilesXY.Count > 0)
                {
                    Tuple<short, short> exists = tilesXY.Find(s => s.Item1 == t.X && s.Item2 == t.Y);
                    if (exists != null) t.IceGrowth = 1;
                }
                if (iceGrowthFixReset) t.IceGrowth = 0; //Overrides ice growth fix

                if (removeLevel0ClearTiles)
                {
                    if (t.TileIndex > 0 || t.Level > 0 || t.SubTileIndex > 0 || t.IceGrowth > 0)
                        tilesetForSort.Add(t);
                }
                else
                {
                    tilesetForSort.Add(t);
                }
            }

            if (tilesetForSort.Count == 0)
            {
                MapTileContainer tile = new MapTileContainer((short)mapLocalWidth, 1, 0, 0, 0, 0);
                tilesetForSort.Add(tile);
            }

            // Sort the tiles before compressing and making IsoMapPack5
            if (isoMapPack5SortBy != IsoMapPack5SortMode.NotDefined)
            {
                Logger.Info("IsoMapPack5SortBy set: Will attempt to sort IsoMapPack5 data using sorting mode: " + isoMapPack5SortBy);
                switch (isoMapPack5SortBy)
                {
                    case IsoMapPack5SortMode.XLevelTileIndex:
                        tilesetSorted = tilesetForSort.OrderBy(x => x.X).ThenBy(x => x.Level).ThenBy(x => x.TileIndex).ToList();
                        break;
                    case IsoMapPack5SortMode.XTileIndexLevel:
                        tilesetSorted = tilesetForSort.OrderBy(x => x.X).ThenBy(x => x.TileIndex).ThenBy(x => x.Level).ToList();
                        break;
                    case IsoMapPack5SortMode.TileIndexXLevel:
                        tilesetSorted = tilesetForSort.OrderBy(x => x.TileIndex).ThenBy(x => x.X).ThenBy(x => x.Level).ToList();
                        break;
                    case IsoMapPack5SortMode.LevelXTileIndex:
                        tilesetSorted = tilesetForSort.OrderBy(x => x.Level).ThenBy(x => x.X).ThenBy(x => x.TileIndex).ToList();
                        break;
                    case IsoMapPack5SortMode.X:
                        tilesetSorted = tilesetForSort.OrderBy(x => x.X).ToList();
                        break;
                    case IsoMapPack5SortMode.Level:
                        tilesetSorted = tilesetForSort.OrderBy(x => x.Level).ToList();
                        break;
                    case IsoMapPack5SortMode.TileIndex:
                        tilesetSorted = tilesetForSort.OrderBy(x => x.TileIndex).ToList();
                        break;
                    case IsoMapPack5SortMode.SubTileIndex:
                        tilesetSorted = tilesetForSort.OrderBy(x => x.SubTileIndex).ToList();
                        break;
                    case IsoMapPack5SortMode.IceGrowth:
                        tilesetSorted = tilesetForSort.OrderBy(x => x.IceGrowth).ToList();
                        break;
                    case IsoMapPack5SortMode.Y:
                        tilesetSorted = tilesetForSort.OrderBy(x => x.Y).ToList();
                        break;
                    default:
                        tilesetSorted.AddRange(tilesetForSort);
                        break;
                }
                TileSetToMapPack(tilesetSorted);
            }
            else
            {
                TileSetToMapPack(tilesetForSort);
            }
            Altered = true;
        }

        /// <summary>
        /// Converts tileset data into compressed IsoMapPack5 format.
        /// </summary>
        private void TileSetToMapPack(List<MapTileContainer> tileSet)
        {
            byte[] isoMapPack = new byte[tileSet.Count * 11 + 4];
            int i = 0;

            foreach (MapTileContainer t in tileSet)
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

            byte[] lzo = Format5.Encode(isoMapPack, 5);
            string data = Convert.ToBase64String(lzo, Base64FormattingOptions.None);
            OverrideBase64MapSection("IsoMapPack5", data);
        }

        /// <summary>
        /// Changes overlay data of current map based on conversion profile.
        /// </summary>
        public void ConvertOverlayData()
        {
            if (!Initialized || overlayRules == null || overlayRules.Count < 1) return;
            else if (!IsCurrentTheaterAllowed())
            {
                Logger.Warn("Skipping altering overlay data - ApplicableTheaters does not contain entry matching map theater.");
                return;
            }
            ParseOverlayPack();
            Logger.Info("Attempting to modify overlay data of the map file.");
            ApplyOverlayConversionRules();
        }


        /// <summary>
        /// Processes overlay data conversion rules.
        /// </summary>
        private void ApplyOverlayConversionRules()
        {
            Random random = new Random();
            for (int i = 0; i < Math.Min(overlayPack.Length, overlayDataPack.Length); i++)
            {
                if (overlayPack[i] == 255) continue;
                if (overlayPack[i] < 0 || overlayPack[i] > 255) overlayPack[i] = 0;
                if (overlayDataPack[i] < 0 || overlayDataPack[i] > 255) overlayDataPack[i] = 0;
                foreach (ByteIDConversionRule rule in overlayRules)
                {
                    if (!rule.ValidForOverlays()) continue;
                    if (overlayPack[i] >= rule.OriginalStartIndex && overlayPack[i] <= rule.OriginalEndIndex)
                    {
                        if (rule.IsRandomizer)
                        {
                            byte newindex = (byte)random.Next(rule.NewStartIndex, rule.NewEndIndex);
                            Logger.Debug("OverlayRules: Random range [" + rule.NewStartIndex + "-" + rule.NewEndIndex + "]. Picked: " + newindex);
                            if (newindex != overlayPack[i])
                            {
                                Logger.Debug("OverlayRules: Overlay ID '" + overlayPack[i] + " at array slot " + i + "' changed to '" + newindex + "'.");
                                overlayPack[i] = newindex;
                            }
                            break;
                        }
                        else if (rule.NewEndIndex == rule.NewStartIndex)
                        {
                            Logger.Debug("OverlayRules: Overlay ID '" + overlayPack[i] + " at array slot " + i + "' changed to '" + rule.NewStartIndex + "'.");
                            overlayPack[i] = (byte)rule.NewStartIndex;
                            break;
                        }
                        else
                        {
                            Logger.Debug("OverlayRules: Overlay ID '" + overlayPack[i] + " at array slot " + i + "' changed to '" +
                                (rule.NewStartIndex + Math.Abs(rule.OriginalStartIndex - overlayPack[i])) + "'.");
                            overlayPack[i] = (byte)(rule.NewStartIndex + Math.Abs(rule.OriginalStartIndex - overlayPack[i]));
                            break;
                        }
                    }
                }
            }
            SaveOverlayPack();
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
                foreach (StringIDConversionRule rule in objectRules)
                {
                    if (rule == null || rule.Original == null) continue;
                    if (CheckIfObjectIDMatches(kvp.Value, rule.Original))
                    {
                        if (rule.New == null)
                        {
                            Logger.Debug("ObjectRules: Removed " + sectionName + " object with ID '" + rule.Original + "' from the map file.");
                            mapINI.RemoveKey(sectionName, kvp.Key);
                            Altered = true;
                        }
                        else
                        {
                            Logger.Debug("ObjectRules: Replaced " + sectionName + " object with ID '" + rule.Original + "' with object of ID '" + rule.New + "'.");
                            mapINI.SetKey(sectionName, kvp.Key, kvp.Value.Replace(rule.Original, rule.New));
                            Altered = true;
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
            List<string> matchingKeys = new List<string>();
            foreach (MapTileContainer tile in isoMapPack5)
            {
                string key = (tile.X + (1000 * tile.Y)).ToString();
                if (keys.Contains(key))
                {
                    matchingKeys.Add(key);
                }
            }
            foreach (string key in keys)
            {
                if (!matchingKeys.Contains(key))
                {
                    Logger.Debug("DeleteObjectsOutsideMapBounds: Removed Terrain object with ID '" + mapINI.GetKey("Terrain", key, "") +
                        "' from the map file, because it was out of map bounds.");
                    mapINI.RemoveKey("Terrain", key);
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
                int X = Conversion.GetIntFromString(tmp[3], -1);
                int Y = Conversion.GetIntFromString(tmp[4], -1);
                if (X < 0 || Y < 0)
                    continue;
                MapTileContainer tile = isoMapPack5.Find(x => x.X == X && x.Y == Y);
                if (tile == null)
                {
                    Logger.Debug("DeleteObjectsOutsideMapBounds: Removed " + sectionName + " object with ID '" + mapINI.GetKey(sectionName, key, "") +
                        "' from the map file, because it was out of map bounds.");
                    mapINI.RemoveKey(sectionName, key);
                }
            }
        }

        /// <summary>
        /// Deletes overlays outside map bounds.
        /// </summary>
        private void DeleteOverlaysOutsideBounds()
        {
            if (overlayPack == null || overlayDataPack == null)
                ParseOverlayPack();

            if (overlayPack == null || overlayDataPack == null)
                return;

            byte[] newOverlayPack = Enumerable.Repeat<byte>(255, overlayPack.Length).ToArray();
            byte[] newOverlayDataPack = Enumerable.Repeat<byte>(0, overlayDataPack.Length).ToArray();

            foreach (MapTileContainer tile in isoMapPack5)
            {
                int index = tile.X + (512 * tile.Y);
                newOverlayPack[index] = overlayPack[index];
                newOverlayDataPack[index] = overlayDataPack[index];
            }
            overlayPack = newOverlayPack;
            overlayDataPack = newOverlayDataPack;
            SaveOverlayPack();
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
                    Altered = true;
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
                    Altered = true;
                    currentSection = rule.NewSection;
                }

                string currentKey = rule.OriginalKey;
                if (rule.NewKey == null)
                {
                    Logger.Debug("SectionRules: Removed key '" + rule.OriginalKey + "' from section '" + currentSection + "'.");
                    mapINI.RemoveKey(currentSection, rule.OriginalKey);
                    Altered = true;
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
                    Altered = true;
                    currentKey = rule.NewKey;
                }

                if (rule.NewValue != "")
                {
                    Logger.Debug("SectionRules: Section '" + currentSection + "' key '" + currentKey + "' value changed to '" + rule.NewValue + "'.");
                    mapINI.SetKey(currentSection, currentKey, rule.NewValue);
                    Altered = true;
                }
            }
        }

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
        /// Lists theater config file data to a text file.
        /// </summary>
        public void ListTileSetData()
        {
            if (!Initialized || theaterConfigINI == null) return;

            TilesetCollection mtiles = TilesetCollection.ParseFromINIFile(theaterConfigINI);

            if (mtiles == null || mtiles.Count < 1)
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
            foreach (Tileset ts in mtiles)
            {
                if (ts.TilesInSet < 1)
                {
                    Logger.Debug("ListTileSetData: " + ts.SetID + " (" + ts.SetName + ")" + " skipped due to tile count of 0.");
                    continue;
                }
                lines.AddRange(ts.GetPrintableData(tilecounter));
                lines.Add("");
                tilecounter += ts.TilesInSet;
                Logger.Debug("ListTileSetData: " + ts.SetID + " (" + ts.SetName + ")" + " added to the list.");
            }
            File.WriteAllLines(filenameOutput, lines.ToArray());
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

    }
}
