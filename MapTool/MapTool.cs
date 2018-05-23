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
using System.Text;
using System.IO;
using CNCMaps.FileFormats.Encodings;
using CNCMaps.FileFormats.VirtualFileSystem;
using StarkkuUtils.FileTypes;
using StarkkuUtils.Utilities;
using System.Text.RegularExpressions;

namespace MapTool
{
    class MapTool
    {

        // Tool initialized true/false.
        public bool Initialized
        {
            get;
            set;
        }

        // Map file altered true/false.
        public bool Altered
        {
            get;
            set;
        }

        private string FileInput;
        private string FileOutput;

        private INIFile MapConfig;                                                                // Map file.
        private string MapTheater = null;                                                         // Map theater data.
        private int Map_Width;
        private int Map_Height;
        private int Map_FullWidth;
        private int Map_FullHeight;
        private List<MapTileContainer> IsoMapPack5 = new List<MapTileContainer>();                // Map tile data.
        private byte[] OverlayPack = null;                                                        // Map overlay ID data.
        private byte[] OverlayDataPack = null;                                                    // Map overlay frame data.

        private INIFile ProfileConfig;                                                            // Conversion profile INI file.
        private List<string> ApplicableTheaters = new List<string>();                             // Conversion profile applicable theaters.
        private string NewTheater = null;                                                         // Conversion profile new theater.
        private List<ByteIDConversionRule> TileRules = new List<ByteIDConversionRule>();          // Conversion profile tile rules.
        private List<ByteIDConversionRule> OverlayRules = new List<ByteIDConversionRule>();       // Conversion profile overlay rules.
        private List<StringIDConversionRule> ObjectRules = new List<StringIDConversionRule>();    // Conversion profile object rules.
        private List<SectionConversionRule> SectionRules = new List<SectionConversionRule>();     // Conversion profile section rules.

        // Map modify options.
        private bool UseMapOptimize = false;
        private bool UseMapCompress = false;
        private string IsoMapPack5SortBy = null;
        private bool RemoveLevel0ClearTiles = false;
        private string IceGrowthFixUseBuilding = null;
        private bool IceGrowthFixReset = false;

        private INIFile TheaterConfig;                                                            // Theater config INI file.

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
            FileInput = inputFile;
            FileOutput = outputFile;

            if (listTheaterData && !String.IsNullOrEmpty(FileInput))
            {
                TheaterConfig = new INIFile(FileInput);
                if (!TheaterConfig.Initialized)
                {
                    Initialized = false;
                    return;
                }
            }

            else if (!String.IsNullOrEmpty(FileInput) && !String.IsNullOrEmpty(FileOutput))
            {

                Logger.Info("Reading map file '" + inputFile + "'.");
                MapConfig = new INIFile(inputFile);
                if (!MapConfig.Initialized)
                {
                    Initialized = false;
                    return;
                }
                string[] size = MapConfig.GetKey("Map", "Size", "").Split(',');
                Map_FullWidth = int.Parse(size[2]);
                Map_FullHeight = int.Parse(size[3]);
                Initialized = ParseMapPack();
                MapTheater = MapConfig.GetKey("Map", "Theater", null);
                if (MapTheater != null) MapTheater = MapTheater.ToUpper();

                ProfileConfig = new INIFile(fileConfig);
                if (!ProfileConfig.Initialized) Initialized = false;
                else
                {
                    Logger.Info("Parsing conversion profile file.");

                    string include_string = ProfileConfig.GetKey("ProfileData", "IncludeFiles", null);
                    if (!String.IsNullOrEmpty(include_string))
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
                                ProfileConfig.Merge(ic);
                            }
                        }
                    }

                    UseMapOptimize = Conversion.GetBoolFromString(ProfileConfig.GetKey("ProfileData", "ApplyMapOptimization", "false"), false);
                    UseMapCompress = Conversion.GetBoolFromString(ProfileConfig.GetKey("ProfileData", "ApplyMapCompress", "false"), false);
                    IsoMapPack5SortBy = ProfileConfig.GetKey("IsoMapPack5", "SortBy", null);
                    if (IsoMapPack5SortBy != null)
                        IsoMapPack5SortBy = IsoMapPack5SortBy.ToUpper();
                    RemoveLevel0ClearTiles = Conversion.GetBoolFromString(ProfileConfig.GetKey("IsoMapPack5", "RemoveLevel0ClearTiles", "false"), false);
                    IceGrowthFixUseBuilding = ProfileConfig.GetKey("IsoMapPack5", "IceGrowthFixUseBuilding", null);
                    IceGrowthFixReset = Conversion.GetBoolFromString(ProfileConfig.GetKey("IsoMapPack5", "IceGrowthFixReset", "false"), false);

                    string[] tilerules = null;
                    string[] overlayrules = null;
                    string[] objectrules = null;
                    string[] sectionrules = null;

                    if (ProfileConfig.SectionExists("TileRules")) tilerules = ProfileConfig.GetValues("TileRules");
                    if (ProfileConfig.SectionExists("OverlayRules")) overlayrules = ProfileConfig.GetValues("OverlayRules");
                    if (ProfileConfig.SectionExists("ObjectRules")) objectrules = ProfileConfig.GetValues("ObjectRules");
                    if (ProfileConfig.SectionExists("SectionRules")) sectionrules = MergeKeyValuePairs(ProfileConfig.GetKeyValuePairs("SectionRules"));

                    NewTheater = ProfileConfig.GetKey("TheaterRules", "NewTheater", null);
                    if (NewTheater != null)
                        NewTheater = NewTheater.ToUpper();

                    string[] applicableTheaters = null;
                    applicableTheaters = ProfileConfig.GetKey("TheaterRules", "ApplicableTheaters", "").Split(',');
                    if (applicableTheaters != null)
                    {
                        for (int i = 0; i < applicableTheaters.Length; i++)
                        {
                            string theater = applicableTheaters[i].Trim().ToUpper();
                            if (theater == "") continue;
                            ApplicableTheaters.Add(theater);
                        }
                    }
                    if (ApplicableTheaters.Count < 1)
                        ApplicableTheaters.AddRange(new string[] { "TEMPERATE", "SNOW", "URBAN", "DESERT", "LUNAR", "NEWURBAN" });

                    // Allow saving map without any other changes if either of these are set and ApplicableTheaters allows it.
                    if ((UseMapCompress || UseMapOptimize) && IsCurrentTheaterAllowed()) Altered = true;

                    if (!Altered && tilerules == null && overlayrules == null && objectrules == null && sectionrules == null && String.IsNullOrEmpty(NewTheater))
                    {
                        Logger.Error("No conversion rules to apply in conversion profile file. Aborting.");
                        Initialized = false;
                        return;
                    }

                    ParseConfigFile(tilerules, TileRules);
                    ParseConfigFile(overlayrules, OverlayRules);
                    ParseConfigFile(objectrules, ObjectRules);
                    ParseConfigFile(sectionrules, SectionRules);
                }
            }
            Initialized = true;
        }

        /// <summary>
        /// Saves the map file.
        /// </summary>
        public void Save()
        {
            if (UseMapOptimize)
            {
                Logger.Info("ApplyMapOptimization set: Saved map will have map section order optimizations applied.");
                MapConfig.MoveSectionToFirst("Basic");
                MapConfig.MoveSectionToFirst("MultiplayerDialogSettings");
                MapConfig.MoveSectionToLast("Digest");
            }
            if (UseMapCompress)
                Logger.Info("ApplyMapCompress set: Saved map will have no unnecessary whitespaces.");
            MapConfig.Save(FileOutput, !UseMapCompress);
        }

        /// <summary>
        /// Checks if theater name is valid.
        /// </summary>
        /// <param name="theaterName">Theater name.</param>
        /// <returns>True if a valid theater name, otherwise false.</returns>
        public static bool IsValidTheaterName(string theaterName)
        {
            if (theaterName == "TEMPERATE" || theaterName == "SNOW" || theaterName == "LUNAR" || theaterName == "DESERT" || theaterName == "URBAN" || theaterName == "NEWURBAN") return true;
            return false;
        }

        /// <summary>
        /// Checks if the currently set map theater exists in current list of theaters the map tool is allowed to process.
        /// </summary>
        /// <returns>True if map theater exists in applicable theaters, otherwise false.</returns>
        private bool IsCurrentTheaterAllowed()
        {
            if (ApplicableTheaters == null || MapTheater == null || !ApplicableTheaters.Contains(MapTheater)) return false;
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
            string[] tmp = MapConfig.GetValues("IsoMapPack5");
            if (tmp == null || tmp.Length < 1) return false;
            data = String.Join("", tmp);
            int cells;
            byte[] isoMapPack;
            try
            {
                string size = MapConfig.GetKey("Map", "Size", "");
                string[] st = size.Split(',');
                Map_Width = Convert.ToInt16(st[2]);
                Map_Height = Convert.ToInt16(st[3]);
                byte[] lzoData = Convert.FromBase64String(data);
                byte[] test = lzoData;
                cells = (Map_Width * 2 - 1) * Map_Height;
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
                int dx = x - y + Map_FullWidth - 1;
                int dy = x + y - Map_FullWidth - 1;
                if (x > 0 && y > 0 && x <= 16384 && y <= 16384)
                {
                    IsoMapPack5.Add(new MapTileContainer((short)x, (short)y, tileNum, subTile, level, iceGrowth));
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
            string[] values = MapConfig.GetValues("OverlayPack");
            if (values == null || values.Length < 1) return;
            byte[] format80Data = Convert.FromBase64String(String.Join("", values));
            var overlaypack = new byte[1 << 18];
            Format5.DecodeInto(format80Data, overlaypack, 80);

            Logger.Info("Parsing OverlayDataPack.");
            values = MapConfig.GetValues("OverlayDataPack");
            if (values == null || values.Length < 1) return;
            format80Data = Convert.FromBase64String(String.Join("", values));
            var overlaydatapack = new byte[1 << 18];
            Format5.DecodeInto(format80Data, overlaydatapack, 80);

            OverlayPack = overlaypack;
            OverlayDataPack = overlaydatapack;
        }

        /// <summary>
        /// Saves Overlay(Data)Pack section(s) of the map file.
        /// </summary>
        private void SaveOverlayPack()
        {
            string base64_overlayPack = Convert.ToBase64String(Format5.Encode(OverlayPack, 80), Base64FormattingOptions.None);
            string base64_overlayDataPack = Convert.ToBase64String(Format5.Encode(OverlayDataPack, 80), Base64FormattingOptions.None);
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
            MapConfig.ReplaceSectionValues(sectionName, lines);
        }

        /// <summary>
        /// Parses conversion profile information for byte ID-type rules.
        /// </summary>
        private void ParseConfigFile(string[] newRules, List<ByteIDConversionRule> currentRules)
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
                    if (sec.Length == 1 && values[0].Contains('=') || sec.Length > 1 && values[0].Contains('=') && String.IsNullOrEmpty(sec[1])) newSection = null;
                    else if (sec.Length > 1) newSection = sec[1];
                    if (values.Length > 1)
                    {
                        string[] key = values[1].Split('=');
                        if (key != null && key.Length > 0)
                        {
                            originalKey = key[0];
                            if (key.Length == 1 && values[1].Contains('=') || key.Length > 1 && values[1].Contains('=') && String.IsNullOrEmpty(key[1])) newKey = null;
                            else if (key.Length > 1) newKey = key[1];
                        }
                        if (values.Length > 2)
                        {
                            if (!(values[2] == null || values[2] == "" || values[2] == "*"))
                            {
                                if (values[2].StartsWith("$GETVAL") && values[2].Contains('(') && values[2].Contains(')'))
                                {
                                    string[] valdata = Regex.Match(values[2], @"\(([^)]*)\)").Groups[1].Value.Split(',');
                                    if (valdata.Length > 1)
                                    {
                                        string newval = MapConfig.GetKey(valdata[0], valdata[1], null);
                                        if (newval != null) newValue = newval;
                                    }
                                }
                                else newValue = values[2];
                            }
                        }
                    }
                    currentRules.Add(new SectionConversionRule(originalSection, newSection, originalKey, newKey, newValue));
                }
            }
        }

        /// <summary>
        /// Changes theater declaration of current map based on conversion profile.
        /// </summary>
        public void ConvertTheaterData()
        {
            if (!Initialized || String.IsNullOrEmpty(NewTheater)) return;
            else if (!IsCurrentTheaterAllowed())
            {
                Logger.Warn("Skipping altering theater data - ApplicableTheaters does not contain entry matching map theater.");
                return;
            }
            Logger.Info("Attempting to modify theater data of the map file.");
            if (IsValidTheaterName(NewTheater))
            {
                MapConfig.SetKey("Map", "Theater", NewTheater);
                Logger.Info("Map theater declaration changed from '" + MapTheater + "' to '" + NewTheater + "'.");
                Altered = true;
            }
        }

        /// <summary>
        /// Changes tile data of current map based on conversion profile.
        /// </summary>
        public void ConvertTileData()
        {
            if (!Initialized || IsoMapPack5.Count < 1 || TileRules == null || TileRules.Count < 1) return;
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
            foreach (MapTileContainer tile in IsoMapPack5)
            {
                if (tile.TileIndex < 0 || tile.TileIndex == 65535) tile.TileIndex = 0;
                foreach (ByteIDConversionRule rule in TileRules)
                {
                    if (tile.TileIndex >= rule.OriginalStartIndex && tile.TileIndex <= rule.OriginalEndIndex)
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
                            int newindex = random.Next(rule.NewStartIndex, rule.NewEndIndex);
                            Logger.Debug("Tile rule random range: [" + rule.NewStartIndex +"-" + rule.NewEndIndex+ "]. Picked: " + newindex);
                            if (newindex != tile.TileIndex)
                            {
                                Logger.Debug("Tile ID " + tile.TileIndex + " at " + tile.X + "," + tile.Y + " changed to " + newindex);
                                tile.TileIndex = newindex;
                            }
                            break;
                        }
                        else if (rule.NewEndIndex == rule.NewStartIndex)
                        {
                            Logger.Debug("Tile ID " + tile.TileIndex + " at " + tile.X + "," + tile.Y + " changed to " + rule.NewStartIndex);
                            tile.TileIndex = rule.NewStartIndex;
                            break;
                        }
                        else
                        {
                            Logger.Debug("Tile ID " + tile.TileIndex + " at " + tile.X + "," + tile.Y + " changed to " + 
                                (rule.NewStartIndex + Math.Abs(rule.OriginalStartIndex - tile.TileIndex)));
                            tile.TileIndex = (rule.NewStartIndex + Math.Abs(rule.OriginalStartIndex - tile.TileIndex));
                            break;
                        }
                    }
                }
            }

            // Fix for TS Snow Maps Ice Growth, FinalSun sets all IceGrowth byte to 0
            // Using a defined building to get a list of X, Y then to set IceGrowth to 1
            string[] buildings = MapConfig.GetValues("Structures");
            if (IceGrowthFixUseBuilding != null && buildings != null && buildings.Length > 0)
            {
                foreach (string building in buildings)
                {
                    string[] values = building.Split(',');
                    if (values != null && values.Length > 1)
                    {
                        string buildingID = values[1].Trim();
                        if (buildingID != "" && buildingID == IceGrowthFixUseBuilding)
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
            if (IceGrowthFixReset)
                Logger.Info("IceGrowthFixReset set: Will attempt to disable ice growth for entire map.");
            else if (tilesXY.Count > 0)
                Logger.Info("IceGrowthFixUseBuilding set: Will attempt to enable ice growth for tiles with coordinates from building ID: " + IceGrowthFixUseBuilding);
            else if (IceGrowthFixUseBuilding != null && tilesXY.Count < 1)
                Logger.Warn("IceGrowthFixUseBuilding is set but no instances of the building were found on the map.");

            if (RemoveLevel0ClearTiles)
                Logger.Info("RemoveLevel0ClearTiles set: Will attempt to remove all tile data with tile index & level set to 0");

            // Remove Height Level 0 Clear Tiles if set in profile
            foreach (MapTileContainer t in IsoMapPack5)
            {
                // Set IceGrowth byte to 1 for Ice Growth for specific tiles. If Reset, set all to 0
                if (tilesXY.Count > 0)
                {
                    Tuple<short, short> exists = tilesXY.Find(s => s.Item1 == t.X && s.Item2 == t.Y);
                    if (exists != null) t.IceGrowth = 1;
                }
                if (IceGrowthFixReset) t.IceGrowth = 0; //Overrides ice growth fix

                if (RemoveLevel0ClearTiles)
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
                MapTileContainer tile = new MapTileContainer((short)Map_Width, 1, 0, 0, 0, 0);
                tilesetForSort.Add(tile);
            }

            // Sort the tiles before compressing and making IsoMapPack5
            if (IsoMapPack5SortBy != null)
            {
                Logger.Info("IsoMapPack5SortBy set: Will attempt to sort IsoMapPack5 data using sorting mode: " + IsoMapPack5SortBy);
                switch (IsoMapPack5SortBy)
                {
                    case "X_LEVEL_TILEINDEX":
                        tilesetSorted = tilesetForSort.OrderBy(x => x.X).ThenBy(x => x.Level).ThenBy(x => x.TileIndex).ToList();
                        break;
                    case "X_TILEINDEX_LEVEL":
                        tilesetSorted = tilesetForSort.OrderBy(x => x.X).ThenBy(x => x.TileIndex).ThenBy(x => x.Level).ToList();
                        break;
                    case "TILEINDEX_X_LEVEL":
                        tilesetSorted = tilesetForSort.OrderBy(x => x.TileIndex).ThenBy(x => x.X).ThenBy(x => x.Level).ToList();
                        break;
                    case "LEVEL_X_TILEINDEX":
                        tilesetSorted = tilesetForSort.OrderBy(x => x.Level).ThenBy(x => x.X).ThenBy(x => x.TileIndex).ToList();
                        break;
                    case "X":
                        tilesetSorted = tilesetForSort.OrderBy(x => x.X).ToList();
                        break;
                    case "LEVEL":
                        tilesetSorted = tilesetForSort.OrderBy(x => x.Level).ToList();
                        break;
                    case "TILEINDEX":
                        tilesetSorted = tilesetForSort.OrderBy(x => x.TileIndex).ToList();
                        break;
                    case "SUBTILEINDEX":
                        tilesetSorted = tilesetForSort.OrderBy(x => x.SubTileIndex).ToList();
                        break;
                    case "ICEGROWTH":
                        tilesetSorted = tilesetForSort.OrderBy(x => x.IceGrowth).ToList();
                        break;
                    case "Y":
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
            if (!Initialized || OverlayRules == null || OverlayRules.Count < 1) return;
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
            for (int i = 0; i < Math.Min(OverlayPack.Length, OverlayDataPack.Length); i++)
            {
                if (OverlayPack[i] == 255) continue;
                if (OverlayPack[i] < 0 || OverlayPack[i] > 255) OverlayPack[i] = 0;
                if (OverlayDataPack[i] < 0 || OverlayDataPack[i] > 255) OverlayDataPack[i] = 0;
                foreach (ByteIDConversionRule rule in OverlayRules)
                {
                    if (!rule.ValidForOverlays()) continue;
                    if (OverlayPack[i] >= rule.OriginalStartIndex && OverlayPack[i] <= rule.OriginalEndIndex)
                    {
                        if (rule.IsRandomizer)
                        {
                            byte newindex = (byte)random.Next(rule.NewStartIndex, rule.NewEndIndex);
                            Logger.Debug("Overlay rule random range: [" + rule.NewStartIndex + "-" + rule.NewEndIndex + "]. Picked: " + newindex);
                            if (newindex != OverlayPack[i])
                            {
                                Logger.Debug("Overlay ID '" + OverlayPack[i] + " at array slot " + i + "' changed to '" + newindex + "'.");
                                OverlayPack[i] = newindex;
                            }
                            break;
                        }
                        else if (rule.NewEndIndex == rule.NewStartIndex)
                        {
                            Logger.Debug("Overlay ID '" + OverlayPack[i] + " at array slot " + i + "' changed to '" + rule.NewStartIndex + "'.");
                            OverlayPack[i] = (byte)rule.NewStartIndex;
                            break;
                        }
                        else
                        {
                            Logger.Debug("Overlay ID '" + OverlayPack[i] + " at array slot " + i + "' changed to '" + (rule.NewStartIndex + Math.Abs(rule.OriginalStartIndex - OverlayPack[i])) + "'.");
                            OverlayPack[i] = (byte)(rule.NewStartIndex + Math.Abs(rule.OriginalStartIndex - OverlayPack[i]));
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
            if (!Initialized || OverlayRules == null || ObjectRules.Count < 1) return;
            else if (MapTheater != null && ApplicableTheaters != null && !ApplicableTheaters.Contains(MapTheater)) { Logger.Warn("Conversion profile not applicable to maps belonging to this theater. No alterations will be made to the object data."); return; }
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
            if (String.IsNullOrEmpty(sectionName)) return;
            KeyValuePair<string, string>[] kvps = MapConfig.GetKeyValuePairs(sectionName);
            if (kvps == null) return;
            foreach (KeyValuePair<string, string> kvp in kvps)
            {
                foreach (StringIDConversionRule rule in ObjectRules)
                {
                    if (rule == null || rule.Original == null) continue;
                    if (CheckIfObjectIDMatches(kvp.Value, rule.Original))
                    {
                        if (rule.New == null)
                        {
                            Logger.Debug("Removed " + sectionName + " object with ID '" + rule.Original + "' from the map file.");
                            MapConfig.RemoveKey(sectionName, kvp.Key);
                            Altered = true;
                        }
                        else
                        {
                            Logger.Debug("Replaced " + sectionName + " object with ID '" + rule.Original + "' with object of ID '" + rule.New + "'.");
                            MapConfig.SetKey(sectionName, kvp.Key, kvp.Value.Replace(rule.Original, rule.New));
                            Altered = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Changes section data of current map based on conversion profile.
        /// </summary>
        public void ConvertSectionData()
        {
            if (!Initialized || SectionRules == null || SectionRules.Count < 1) return;
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
            foreach (SectionConversionRule rule in SectionRules)
            {
                if (String.IsNullOrEmpty(rule.OriginalSection)) continue;

                string currentSection = rule.OriginalSection;
                if (rule.NewSection == null)
                {
                    MapConfig.RemoveSection(rule.OriginalSection);
                    Altered = true;
                    continue;
                }
                else if (rule.NewSection != "")
                {
                    if (!MapConfig.SectionExists(rule.OriginalSection)) MapConfig.AddSection(rule.NewSection);
                    else MapConfig.RenameSection(rule.OriginalSection, rule.NewSection);
                    Altered = true;
                    currentSection = rule.NewSection;
                }

                string currentKey = rule.OriginalKey;
                if (rule.NewKey == null)
                {
                    MapConfig.RemoveKey(currentSection, rule.OriginalKey);
                    Altered = true;
                    continue;
                }
                else if (rule.NewKey != "")
                {
                    if (MapConfig.GetKey(currentSection, rule.OriginalKey, null) == null) MapConfig.SetKey(currentSection, rule.NewKey, "");
                    else MapConfig.RenameKey(currentSection, rule.OriginalKey, rule.NewKey);
                    Altered = true;
                    currentKey = rule.NewKey;
                }

                if (rule.NewValue != "")
                {
                    MapConfig.SetKey(currentSection, currentKey, rule.NewValue);
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
            if (!Initialized || TheaterConfig == null) return;

            TilesetCollection mtiles = TilesetCollection.ParseFromINIFile(TheaterConfig);

            if (mtiles == null || mtiles.Count < 1) { Logger.Error("Could not parse tileset data from theater configuration file '" + TheaterConfig.Filename + "'."); return; };

            Logger.Info("Attempting to list tileset data for a theater based on file: '" + TheaterConfig.Filename + "'.");
            List<string> lines = new List<string>();
            int tilecounter = 0;
            lines.Add("Theater tileset data gathered from file '" + TheaterConfig.Filename + "'.");
            lines.Add("");
            lines.Add("");
            foreach (Tileset ts in mtiles)
            {
                if (ts.TilesInSet < 1)
                {
                    Logger.Debug(ts.SetID + " (" + ts.SetName + ")" + " skipped due to tile count of 0.");
                    continue;
                }
                lines.AddRange(ts.GetPrintableData(tilecounter));
                lines.Add("");
                tilecounter += ts.TilesInSet;
                Logger.Debug(ts.SetID + " (" + ts.SetName + ")" + " added to the list.");
            }
            File.WriteAllLines(FileOutput, lines.ToArray());
        }

        /// <summary>
        /// Merges array of string key-value pairs to a single string array containing strings of the keys and values separated by =.
        /// </summary>
        /// <param name="keyValuePairs">Array of string key-value pairs.</param>
        /// <returns>Array of strings made by merging the keys and values.</returns>
        private string[] MergeKeyValuePairs(KeyValuePair<string, string>[] keyValuePairs)
        {
            string[] result = new string[keyValuePairs.Length];
            for (int i = 0; i < keyValuePairs.Length; i++)
            {
                result[i] = keyValuePairs[i].Key + "=" + keyValuePairs[i].Value;
            }
            return result;
        }

    }
}
