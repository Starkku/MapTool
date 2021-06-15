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
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reflection;
using Starkku.Utilities;
using Starkku.Utilities.FileTypes;
using Starkku.Utilities.ExtensionMethods;

namespace MapTool.Logic
{
    /// <summary>
    /// Map tile data sort mode.
    /// </summary>
    public enum IsoMapPack5SortMode { NotDefined, XLevelTileIndex, XTileIndexLevel, TileIndexXLevel, LevelXTileIndex, X, Level, TileIndex, SubTileIndex, IceGrowth, Y }

    /// <summary>
    /// Map file modifier tool class.
    /// </summary>
    public class MapFileTool
    {
        #region public_properties

        /// <summary>
        /// Has tool been initialized or not.
        /// </summary>
        public bool Initialized { get; private set; }

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
        private readonly MapFile map;

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
        private readonly List<ObjectConversionRule> objectRules = new List<ObjectConversionRule>();

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
        public MapFileTool(string inputFile, string outputFile, string fileConfig, bool listTheaterData)
        {
            Initialized = false;
            FilenameInput = inputFile;
            FilenameOutput = outputFile;

            if (listTheaterData && !string.IsNullOrEmpty(FilenameInput))
            {
                theaterConfigINI = new INIFile(FilenameInput);
                Initialized = true;
            }
            else if (!string.IsNullOrEmpty(FilenameInput) && !string.IsNullOrEmpty(FilenameOutput))
            {
                Logger.Info("Initializing map file '" + FilenameInput + "'.");

                map = new MapFile(FilenameInput);

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

                Initialized = true;
            }

        }

        /// <summary>
        /// Saves the map file.
        /// </summary>
        public void Save()
        {
            if (deleteObjectsOutsideMapBounds)
            {
                if (map.FullWidth > 0 && map.FullWidth > 0)
                {
                    Logger.Info("DeleteObjectsOutsideMapBounds set: Objects & overlays outside map bounds will be deleted.");
                    DeleteObjectsOutsideBounds();
                    DeleteOverlaysOutsideBounds();
                }
                else
                    Logger.Warn("DeleteObjectsOutsideMapBounds set but because map has invalid Size value set, no objects or overlays will be deleted.");
            }

            if (useMapOptimize)
            {
                Logger.Info("ApplyMapOptimization set: Saved map will have map section order optimizations applied.");
                map.MoveSectionToFirst("Basic");
                map.MoveSectionToFirst("MultiplayerDialogSettings");
                map.MoveSectionToLast("Digest");
            }

            if (fixTunnels)
            {
                Logger.Info("FixTunnels set: Saved map will have [Tubes] section fixed to remove errors caused by map editor.");
                FixTubesSection();
            }

            if (useMapCompress)
                Logger.Info("ApplyMapCompress set: Saved map will have no unnecessary whitespaces or comments.");

            string error;

            if (map.Altered || useMapCompress)
                error = map.Save(FilenameOutput, !useMapCompress, !useMapCompress);
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
        /// Checks if the currently set map theater exists in current list of theaters the map tool is allowed to process.
        /// </summary>
        /// <returns>True if map theater exists in applicable theaters, otherwise false.</returns>
        private bool IsCurrentTheaterAllowed()
        {
            if (applicableTheaters == null || map.Theater == null || !applicableTheaters.Contains(map.Theater))
                return false;

            return true;
        }

        #region conversion_rule_parsing

        /// <summary>
        /// Parses conversion profile information for tile conversion rules.
        /// </summary>
        /// <param name="ruleStrings">Rules to parse.</param>
        /// <param name="currentRules">Tile conversion rules to replace with parsed rules.</param>
        private void ParseConversionRules(IEnumerable<string> ruleStrings, List<TileConversionRule> currentRules)
        {
            if (ruleStrings == null || !ruleStrings.Any() || currentRules == null)
                return;

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

                int oldSubValueStart = -1, oldSubValueEnd = -1, newSubValueStart = -1, newSubValueEnd = -1;
                bool oldSubValueIsRange = false, newSubValueIsRange = false, newSubValueIsRandom = false;

                if (values.Length >= 6)
                {
                    ParseValueRange(values[5], out oldSubValueStart, out oldSubValueEnd, out oldSubValueIsRange, out _);
                }

                if (values.Length >= 7)
                {
                    ParseValueRange(values[6], out newSubValueStart, out newSubValueEnd, out newSubValueIsRange, out newSubValueIsRandom, true);
                }

                int oldTileEnd = oldValueEnd;
                int newTileEnd = newValueEnd;

                if (oldValueIsRange && !newValueIsRange)
                    newTileEnd = newValueStart + (oldValueEnd - oldValueStart);
                else if (!oldValueIsRange)
                    oldTileEnd = oldValueStart;

                int oldSubEnd = oldSubValueEnd;
                int newSubEnd = newSubValueEnd;

                if (oldSubValueIsRange && !newSubValueIsRange)
                    newSubEnd = newSubValueStart + (oldSubValueEnd - oldSubValueStart);
                else if (!oldSubValueIsRange)
                    oldSubEnd = oldSubValueStart;

                currentRules.Add(new TileConversionRule(oldValueStart, newValueStart, oldTileEnd, newTileEnd, newValueIsRandom,
                    heightOverride, subTileOverride, iceGrowthOverride, coordFilterX, coordFilterY, oldSubValueStart, newSubValueStart, oldSubEnd, newSubEnd, newSubValueIsRandom));
            }
        }

        /// <summary>
        /// Parses conversion profile information for overlay conversion rules.
        /// </summary>
        /// <param name="ruleStrings">Rules to parse.</param>
        /// <param name="currentRules">Overlay conversion rules to replace with parsed rules.</param>
        private void ParseConversionRules(IEnumerable<string> ruleStrings, List<OverlayConversionRule> currentRules)
        {
            if (ruleStrings == null || !ruleStrings.Any() || currentRules == null)
                return;

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
                    frameNewEndIndex = frameNewValueStart + (frameOldValueEnd - frameOldValueStart);
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
        /// Parses conversion profile information for general object conversion rules.
        /// </summary>
        /// <param name="ruleStrings">Rules to parse.</param>
        /// <param name="currentRules">General object conversion rules to replace with parsed rules.</param>
        private void ParseConversionRules(IEnumerable<string> ruleStrings, List<ObjectConversionRule> currentRules)
        {
            if (ruleStrings == null || !ruleStrings.Any() || currentRules == null)
                return;

            currentRules.Clear();

            foreach (string ruleString in ruleStrings)
            {
                string ruleStringFiltered = GetCoordinateFilters(ruleString, out int coordFilterX, out int coordFilterY);

                string[] values = ruleStringFiltered.Split('|');

                if (values.Length == 1)
                {
                    currentRules.Add(new ObjectConversionRule(GetUpgradeIDs(values[0], out List<string> oldUpgradeIDs), GetUpgradeIDs(null, out List<string> newUpgradeIDs),
                        oldUpgradeIDs, newUpgradeIDs, coordFilterX, coordFilterY));
                }
                else if (values.Length >= 2)
                {
                    currentRules.Add(new ObjectConversionRule(GetUpgradeIDs(values[0], out List<string> oldUpgradeIDs), GetUpgradeIDs(values[1], out List<string> newUpgradeIDs),
                        oldUpgradeIDs, newUpgradeIDs, coordFilterX, coordFilterY));
                }
            }
        }

        private string GetUpgradeIDs(string ID, out List<string> upgradeIDs)
        {
            upgradeIDs = new List<string>();

            if (string.IsNullOrEmpty(ID))
                return null;

            string[] split = ID.Split('+');

            if (split.Length < 2)
                return ID;

            string[] uIDs = split[1].Split(',');

            foreach (string uID in uIDs)
            {
                if (string.IsNullOrEmpty(uID))
                    continue;

                upgradeIDs.Add(uID.Trim());
            }

            return split[0].Trim();
        }

        /// <summary>
        /// Parses conversion profile information for map INI section conversion rules.
        /// </summary>
        /// <param name="ruleStrings">Rules to parse.</param>
        /// <param name="currentRules">Map INI section conversion rules to replace with parsed rules.</param>
        private void ParseConversionRules(IEnumerable<string> ruleStrings, List<SectionConversionRule> currentRules)
        {
            if (ruleStrings == null || !ruleStrings.Any() || currentRules == null)
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
                                        string newval = map.GetKey(valdata[0], valdata[1], null);

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
                        valueDouble *= operandDouble;
                        break;
                    case "/":
                        if (operandDouble == 0)
                            operandDouble = 1;
                        valueDouble /= operandDouble;
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
                string coordString = ruleStringFiltered.Substring(1, ruleStringFiltered.IndexOf(")") - 1);
                string[] coords = coordString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                if (coords.Length >= 2)
                {
                    coordFilterX = Conversion.GetIntFromString(coords[0].Replace("*", -1 + ""), -1);
                    coordFilterY = Conversion.GetIntFromString(coords[1].Replace("*", -1 + ""), -1);
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

            string theater = map.Theater;

            if (newTheater.ToUpper() != theater)
            {
                map.Theater = newTheater;

                if (map.Theater != theater)
                    Logger.Info("Map theater declaration changed from '" + map.Theater + "' to '" + newTheater.ToUpper() + "'.");
            }
        }

        /// <summary>
        /// Changes tile data of current map based on conversion profile.
        /// </summary>
        public void ConvertTileData()
        {
            if (!Initialized || !map.HasTileData)
                return;

            if (tileRules.Count < 1 && removeLevel0ClearTiles && isoMapPack5SortBy == IsoMapPack5SortMode.NotDefined)
                return;

            if (map.FullWidth < 1 || map.FullHeight < 1)
            {
                Logger.Error("Could not alter tile data because map size is invalid.");
                return;
            }
            else if (!IsCurrentTheaterAllowed())
            {
                Logger.Warn("Skipping altering tile data - ApplicableTheaters does not contain entry matching map theater.");
                return;
            }

            ApplyTileConversionRules();
            RemoveLevel0ClearTiles();
            SortIsoMapPack();
        }

        /// <summary>
        /// Processes tile data conversion rules.
        /// </summary>
        /// <returns>Returns true if tile data was changed, false if not.</returns>
        private bool ApplyTileConversionRules()
        {
            if (tileRules == null || tileRules.Count < 1)
                return false;

            bool tileDataChanged = false;

            Logger.Info("Attempting to apply TileRules on map tile data.");

            int originalOffset = 0, newOffset = 0;

            if (theaterTileOffsets.ContainsKey(map.Theater))
            {
                originalOffset = theaterTileOffsets[map.Theater].Item1;
                newOffset = theaterTileOffsets[map.Theater].Item2;

                if (newOffset == int.MinValue)
                    newOffset = originalOffset;

                if (originalOffset != 0 && newOffset != 0)
                    Logger.Info("Global tile rule offsets for theater " + map.Theater + ": " + originalOffset + " (original), " + newOffset + " (new)");
            }

            foreach (MapTile tile in map.GetMapTiles())
            {
                if (!tile.IsValid)
                    continue;

                bool tileModified = false;

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
                        if (rule.OriginalSubStartIndex != -1 && rule.OriginalSubEndIndex != -1 &&
                            (tile.SubTileIndex < rule.OriginalSubStartIndex || tile.SubTileIndex > rule.OriginalSubEndIndex))
                            continue;

                        if (rule.HeightOverride > -1)
                        {
                            byte height = (byte)Math.Min(rule.HeightOverride, 14);
                            if (tile.Level != height)
                            {
                                Logger.Debug("TileRules: Tile index " + tile.TileIndex + " at X:" + tile.X + ", Y:" + tile.Y + "  - height changed from " + tile.Level + " to " + height + ".");
                                tile.Level = height;
                                tileModified = true;
                            }
                        }

                        if (rule.SubIndexOverride > -1)
                        {
                            byte subtileIndex = (byte)Math.Min(rule.SubIndexOverride, 255);
                            if (tile.SubTileIndex != subtileIndex)
                            {
                                Logger.Debug("TileRules: Tile index " + tile.TileIndex + " at X:" + tile.X + ", Y:" + tile.Y + " - sub tile index changed from " + tile.SubTileIndex + " to " + subtileIndex + ".");
                                tile.SubTileIndex = subtileIndex;
                                tileModified = true;
                            }
                        }

                        if (rule.IceGrowthOverride > -1)
                        {
                            byte iceGrowth = Convert.ToByte(Convert.ToBoolean(rule.IceGrowthOverride));
                            if (tile.IceGrowth != iceGrowth)
                            {
                                Logger.Debug("TileRules: Tile index " + tile.TileIndex + " at X:" + tile.X + ", Y:" + tile.Y + " - ice growth flag changed from " + tile.IceGrowth + " to " + iceGrowth + ".");
                                tile.IceGrowth = iceGrowth;
                                tileModified = true;
                            }
                        }

                        int newTileIndex = 0;

                        if (rule.IsRandomizer)
                        {
                            newTileIndex = random.Next(ruleNewStartIndex, ruleNewEndIndex);
                            Logger.Debug("TileRules: Tile rule random range: [" + ruleNewStartIndex + "-" + ruleNewEndIndex + "]. Picked: " + newTileIndex);
                        }
                        else if (ruleNewEndIndex == ruleNewStartIndex)
                            newTileIndex = ruleNewStartIndex;
                        else
                            newTileIndex = ruleNewStartIndex + Math.Abs(ruleOriginalStartIndex - tile.TileIndex);

                        if (newTileIndex != tile.TileIndex)
                        {
                            Logger.Debug("TileRules: Tile index " + tile.TileIndex + " at X:" + tile.X + ", Y:" + tile.Y + " - index changed to " + newTileIndex);
                            tile.TileIndex = newTileIndex;
                            tileModified = true;
                        }

                        if (rule.SubIndexOverride < 0 && rule.NewSubStartIndex >= 0 && rule.NewSubEndIndex >= 0)
                        {
                            byte newSubTileIndex = 0;

                            if (rule.IsSubRandomizer)
                            {
                                newSubTileIndex = (byte)random.Next(rule.NewSubStartIndex, rule.NewSubEndIndex);
                                Logger.Debug("TileRules: Tile rule sub-tile random range: [" + rule.NewSubStartIndex + "-" + rule.NewSubEndIndex + "]. Picked: " + newSubTileIndex);
                            }
                            else if (rule.NewSubEndIndex == rule.NewSubStartIndex)
                                newSubTileIndex = (byte)rule.NewSubStartIndex;
                            else
                                newSubTileIndex = (byte)(rule.NewSubStartIndex + Math.Abs(rule.OriginalSubStartIndex - tile.SubTileIndex));

                            if (newSubTileIndex != tile.SubTileIndex)
                            {
                                Logger.Debug("TileRules: Tile sub-tile index " + tile.SubTileIndex + " at X:" + tile.X + ", Y:" + tile.Y + " - index changed to " + newSubTileIndex);
                                tile.SubTileIndex = newSubTileIndex;
                                tileModified = true;
                            }
                        }
                    }

                    tileDataChanged = tileDataChanged || tileModified;

                    if (tileModified)
                        break;
                }
            }

            return tileDataChanged;
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

            foreach (MapTile tile in map.GetMapTiles())
            {
                if (tile.TileIndex < 1 && tile.Level < 1 && tile.SubTileIndex < 1 && tile.IceGrowth < 1)
                    removeTiles.Add(tile);
            }

            return map.RemoveMapTiles(removeTiles) > 0;
        }

        /// <summary>
        /// Sorts tiles in map pack based on the set sorting method.
        /// </summary>
        /// <returns>Returns true if tile data was changed, false if not.</returns>
        private bool SortIsoMapPack()
        {
            if (!Initialized || !map.HasTileData || isoMapPack5SortBy == IsoMapPack5SortMode.NotDefined)
                return false;

            Logger.Info("IsoMapPack5SortBy set: IsoMapPack5 data will be sorted using sorting mode: " + isoMapPack5SortBy);

            List<PropertyInfo> propertyInfos = new List<PropertyInfo>();

            switch (isoMapPack5SortBy)
            {
                case IsoMapPack5SortMode.XLevelTileIndex:
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.X)));
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.X)));
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.Level)));
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.TileIndex)));
                    break;
                case IsoMapPack5SortMode.XTileIndexLevel:
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.X)));
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.TileIndex)));
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.Level)));
                    break;
                case IsoMapPack5SortMode.TileIndexXLevel:
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.TileIndex)));
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.X)));
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.Level)));
                    break;
                case IsoMapPack5SortMode.LevelXTileIndex:
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.Level)));
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.X)));
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.TileIndex)));
                    break;
                case IsoMapPack5SortMode.X:
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.X)));
                    break;
                case IsoMapPack5SortMode.Y:
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.Y)));
                    break;
                case IsoMapPack5SortMode.TileIndex:
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.TileIndex)));
                    break;
                case IsoMapPack5SortMode.SubTileIndex:
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.SubTileIndex)));
                    break;
                case IsoMapPack5SortMode.Level:
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.Level)));
                    break;
                case IsoMapPack5SortMode.IceGrowth:
                    propertyInfos.Add(typeof(MapTile).GetProperty(nameof(MapTile.IceGrowth)));
                    break;
                default:
                    break;
            }

            if (propertyInfos.Count > 0)
                return map.SortMapTileDataByProperties(propertyInfos);

            return false;
        }

        /// <summary>
        /// Changes overlay data of current map based on conversion profile.
        /// </summary>
        public void ConvertOverlayData()
        {
            if (!Initialized || !map.HasOverlayData || overlayRules == null || overlayRules.Count < 1)
                return;

            else if (!IsCurrentTheaterAllowed())
            {
                Logger.Warn("Skipping altering overlay data - ApplicableTheaters does not contain entry matching map theater.");
                return;
            }

            ApplyOverlayConversionRules();
        }

        /// <summary>
        /// Processes overlay data conversion rules.
        /// </summary>
        /// <returns>Returns true if overlay data was changed, false if not.</returns>
        private bool ApplyOverlayConversionRules()
        {
            Logger.Info("Attempting to apply OverlayRules on map overlay data.");

            bool overlayDataChanged = false;

            MapOverlay[] overlays = map.GetMapOverlays();

            for (int i = 0; i < overlays.Length; i++)
            {
                MapOverlay overlay = overlays[i];

                if (overlay.Index < 0 || overlay.Index > 255)
                    overlay.Index = 255;

                if (overlay.FrameIndex < 0 || overlay.FrameIndex > 255)
                    overlay.FrameIndex = 0;

                foreach (OverlayConversionRule rule in overlayRules)
                {
                    if (!rule.IsValid)
                        continue;

                    if (rule.CoordinateFilterX > -1 && rule.CoordinateFilterX != overlay.X ||
                        rule.CoordinateFilterY > -1 && rule.CoordinateFilterY != overlay.Y)
                        continue;

                    bool overlayPackChanged = ChangeOverlayData(overlay, i, rule.OriginalStartIndex, rule.OriginalEndIndex,
                        rule.NewStartIndex, rule.NewEndIndex, rule.IsRandomizer, false);

                    bool overlayDataPackChanged = ChangeOverlayData(overlay, i, rule.OriginalStartFrameIndex, rule.OriginalEndFrameIndex,
                        rule.NewStartFrameIndex, rule.NewEndFrameIndex, rule.IsFrameRandomizer, true);

                    if (overlayPackChanged || overlayDataPackChanged)
                    {
                        overlayDataChanged = true;
                        break;
                    }
                }
            }

            return overlayDataChanged;
        }

        /// <summary>
        /// Changes map overlay data.
        /// </summary>
        /// <param name="overlay">Map overlay.</param>
        /// <param name="index">Map overlay index.</param>
        /// <param name="originalStartIndex">Original start index.</param>
        /// <param name="originalEndIndex">Original end index.</param>
        /// <param name="newStartIndex">New start index.</param>
        /// <param name="newEndIndex">New end index.</param>
        /// <param name="useRandomRange">If true, use a random range.</param>
        /// <param name="changeFrameData">If true, treat changes as being made to frame data rather than overlay ID data.</param>
        /// <returns>Returns true if overlay data was changed, false if not.</returns>
        private bool ChangeOverlayData(MapOverlay overlay, int index, int originalStartIndex, int originalEndIndex,
            int newStartIndex, int newEndIndex, bool useRandomRange, bool changeFrameData)
        {
            string dataType = changeFrameData ? "frame" : "ID";
            int x = overlay.X;
            int y = overlay.Y;
            byte data = changeFrameData ? overlay.FrameIndex : overlay.Index;

            if (data >= originalStartIndex && data <= originalEndIndex)
            {
                if (useRandomRange)
                {
                    byte newIndex = (byte)random.Next(newStartIndex, newEndIndex);
                    Logger.Debug("OverlayRules: Random " + dataType + " range [" + newStartIndex + "-" + newEndIndex + "]. Picked: " + newIndex);
                    if (newIndex != data)
                    {
                        Logger.Debug("OverlayRules: Overlay " + dataType + " " + data + " at array slot " + index + " (X:" + x + ", Y:" + y + ") changed to " +
                            newIndex + ".");

                        if (changeFrameData)
                            overlay.FrameIndex = newIndex;
                        else
                            overlay.Index = newIndex;

                        return true;
                    }
                }
                else if (newEndIndex == newStartIndex)
                {
                    Logger.Debug("OverlayRules: Overlay " + dataType + " " + data + " at array slot " + index + " (X:" + x + ", Y:" + y + ") changed to " +
                        newStartIndex + ".");

                    if (changeFrameData)
                        overlay.FrameIndex = (byte)newStartIndex;
                    else
                        overlay.Index = (byte)newStartIndex;

                    return true;
                }
                else
                {
                    Logger.Debug("OverlayRules: Overlay " + dataType + " " + data + " at array slot " + index + " (X:" + x + ", Y:" + y + ") changed to " +
                        (newStartIndex + Math.Abs(originalStartIndex - data)) + ".");

                    if (changeFrameData)
                        overlay.FrameIndex = (byte)(newStartIndex + Math.Abs(originalStartIndex - data));
                    else
                        overlay.Index = (byte)(newStartIndex + Math.Abs(originalStartIndex - data));

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
            if (!Initialized || overlayRules == null || objectRules.Count < 1)
                return;
            else if (map.Theater != null && applicableTheaters != null && !applicableTheaters.Contains(map.Theater))
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
            Dictionary<string, MapObject> objects = map.GetMapObjectsFromSection(sectionName);

            foreach (KeyValuePair<string, MapObject> kvp in objects)
            {
                MapObject obj = kvp.Value;

                foreach (ObjectConversionRule rule in objectRules)
                {
                    if (rule == null || rule.OriginalName == null)
                        continue;

                    if (rule.CoordinateFilterX > -1 && rule.CoordinateFilterX != obj.X ||
                        rule.CoordinateFilterY > -1 && rule.CoordinateFilterY != obj.Y)
                        continue;

                    if (obj.ID == rule.OriginalName)
                    {
                        HandleUpgrades(obj, rule, out bool skipObject,
                            out string[] oldUpgrades, out string[] newUpgrades, out int oldUpgradeCount, out int newUpgradeCount);

                        if (skipObject)
                            continue;

                        string upgradesOld = oldUpgradeCount > 0 ? " (/w upgrades: '" + string.Join(",", oldUpgrades) + "')" : string.Empty;
                        string upgradesNew = newUpgradeCount > 0 ? " (/w upgrades: '" + string.Join(",", newUpgrades) + "')" : string.Empty;

                        if (rule.NewName == null)
                        {
                            Logger.Debug("ObjectRules: Removed " + sectionName + " object with ID '" + rule.OriginalName + "'" + upgradesOld + "(X:" + obj.X + ", Y:" + obj.Y + ") from the map file.");
                            map.RemoveKey(sectionName, kvp.Key);
                        }
                        else
                        {
                            Logger.Debug("ObjectRules: Replaced " + sectionName + " object with ID '" + rule.OriginalName + "'" + upgradesOld + " (X:" + obj.X + ", Y:" + obj.Y + ") with object of ID '" + rule.NewName + "'" + upgradesNew + ".");
                            obj.ID = rule.NewName;
                            map.SetKey(sectionName, kvp.Key, obj.GetINICode());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles map object upgrades.
        /// </summary>
        /// <param name="mapObject">Map object.</param>
        /// <param name="rule">Object conversion rule.</param>
        /// <param name="skipObject">Set to true if object conversion should be skipped.</param>
        /// <param name="oldUpgrades">Set to list of upgrades currently on the object.</param>
        /// <param name="newUpgrades">Set to list of upgrades that will be on the object after conversion.</param>
        /// <param name="oldUpgradeCount">Set to number of upgrades (excluding empty upgrade slots) currently on the object.</param>
        /// <param name="newUpgradeCount">Set to number of upgrades (excluding empty upgrade slots, unless <paramref name="oldUpgradeCount"/> is larger than zero) that will be on the object after conversion.</param>
        private void HandleUpgrades(MapObject mapObject, ObjectConversionRule rule, out bool skipObject, out string[] oldUpgrades, out string[] newUpgrades, out int oldUpgradeCount, out int newUpgradeCount)
        {
            oldUpgrades = new string[3] { "None", "None", "None" };
            newUpgrades = new string[3] { "None", "None", "None" };
            skipObject = false;
            oldUpgradeCount = 0;
            newUpgradeCount = 0;

            if (mapObject == null || !(mapObject is MapBuildingObject))
                return;

            MapBuildingObject building = mapObject as MapBuildingObject;

            for (int i = 0; i < oldUpgrades.Length; i++)
            {
                string ruleUpgrade = rule.OriginalUpgrades[i];

                if (string.IsNullOrEmpty(ruleUpgrade))
                    continue;

                string buildingUpgrade = building.Upgrades[i];

                if (!ruleUpgrade.Equals("*", StringComparison.InvariantCulture) && !ruleUpgrade.Equals(buildingUpgrade))
                {
                    skipObject = true;
                    return;
                }

                if (!buildingUpgrade.Equals("None", StringComparison.InvariantCulture))
                    oldUpgradeCount++;

                oldUpgrades[i] = buildingUpgrade;
            }

            for (int i = 0; i < newUpgrades.Length; i++)
            {
                string ruleUpgrade = i < rule.NewUpgrades.Length ? rule.NewUpgrades[i] : null;
                string upgrade = building.Upgrades[i];

                if (!string.IsNullOrEmpty(ruleUpgrade) && !ruleUpgrade.Equals("*", StringComparison.InvariantCulture))
                    upgrade = ruleUpgrade;

                if (oldUpgradeCount > 0 || !building.Upgrades[i].Equals("None", StringComparison.InvariantCulture))
                    newUpgradeCount++;

                newUpgrades[i] = upgrade;
            }

            if (newUpgradeCount > 0)
                building.SetUpgrades(newUpgrades);
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
            DeleteObjectsOutsideBoundsFromSection("Terrain");
        }

        /// <summary>
        /// Deletes specific types of objects outside map bounds.
        /// </summary>
        /// <param name="sectionName"></param>
        private void DeleteObjectsOutsideBoundsFromSection(string sectionName)
        {
            Dictionary<string, MapObject> objects = map.GetMapObjectsFromSection(sectionName);

            foreach (KeyValuePair<string, MapObject> kvp in objects)
            {
                MapObject mapObject = kvp.Value;

                if (!map.CoordinateExistsOnMap(mapObject.X, mapObject.Y))
                {
                    Logger.Debug("DeleteObjectsOutsideMapBounds: Removed " + sectionName + " object " + mapObject.ID +
                        " (key: " + kvp.Key + ") from cell " + mapObject.X + "," + mapObject.Y + ".");
                    map.RemoveKey(sectionName, kvp.Key);
                }
            }
        }

        /// <summary>
        /// Deletes overlays outside map bounds.
        /// </summary>
        private void DeleteOverlaysOutsideBounds()
        {
            MapOverlay[] overlays = map.GetMapOverlays();

            for (int i = 0; i < overlays.Length; i++)
            {
                if (overlays[i].Index == 255)
                    continue;

                if (!map.CoordinateExistsOnMap(overlays[i].X, overlays[i].Y))
                {
                    Logger.Debug("DeleteObjectsOutsideMapBounds: Removed overlay (index: " + overlays[i].Index + ") from cell " + overlays[i].X + "," + overlays[i].Y + ".");
                    map.RemoveMapOverlay(overlays[i]);
                }
            }
        }

        /// <summary>
        /// Fixes tunnels.
        /// Based on Rampastring's FinalSun Tunnel Fixer.
        /// https://ppmforums.com/viewtopic.php?t=42008
        /// </summary>
        private void FixTubesSection()
        {
            string[] keys = map.GetKeys("Tubes");

            if (keys == null)
                return;

            int counter = 0;
            foreach (string key in keys)
            {
                List<string> values = map.GetKey("Tubes", key, string.Empty).Split(',').ToList();

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
                map.SetKey("Tubes", key, string.Join(",", values));
                counter++;
            }
        }

        /// <summary>
        /// Changes section data of current map based on conversion profile.
        /// </summary>
        public void ConvertSectionData()
        {
            if (!Initialized || sectionRules == null || sectionRules.Count < 1)
                return;
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
                    map.RemoveSection(rule.OriginalSection);
                    continue;
                }
                else if (rule.NewSection != "")
                {
                    if (!map.SectionExists(rule.OriginalSection))
                    {
                        Logger.Debug("SectionRules: Added new section '" + rule.NewSection + "'.");
                        map.AddSection(rule.NewSection);
                    }
                    else
                    {
                        Logger.Debug("SectionRules: Renamed section '" + rule.OriginalSection + "' to '" + rule.NewSection + "'.");
                        map.RenameSection(rule.OriginalSection, rule.NewSection);
                    }

                    currentSection = rule.NewSection;
                }

                string currentKey = rule.OriginalKey;

                if (rule.NewKey == null)
                {
                    Logger.Debug("SectionRules: Removed key '" + rule.OriginalKey + "' from section '" + currentSection + "'.");
                    map.RemoveKey(currentSection, rule.OriginalKey);
                    continue;
                }
                else if (rule.NewKey != "")
                {
                    if (map.GetKey(currentSection, rule.OriginalKey, null) == null)
                    {
                        Logger.Debug("SectionRules: Added a new key '" + rule.NewKey + "' to section '" + currentSection + "'.");
                        map.SetKey(currentSection, rule.NewKey, "");
                    }
                    else
                    {
                        Logger.Debug("SectionRules: Renamed key '" + rule.OriginalKey + "' in section '" + currentSection + "' to '" + rule.NewKey + "'.");
                        map.RenameKey(currentSection, rule.OriginalKey, rule.NewKey);
                    }

                    currentKey = rule.NewKey;
                }

                if (rule.NewValue != "")
                {
                    Logger.Debug("SectionRules: Section '" + currentSection + "' key '" + currentKey + "' value changed to '" + rule.NewValue + "'.");
                    map.SetKey(currentSection, currentKey, rule.NewValue);
                }
            }
        }

        #endregion

        #region helpers

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
                if (!Regex.IsMatch(section, "^TileSet\\d{4}$"))
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
