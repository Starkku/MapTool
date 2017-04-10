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
using System.Globalization;
using Nini.Config;
using CNCMaps.FileFormats.Encodings;
using CNCMaps.FileFormats.VirtualFileSystem;

namespace MapTool
{
    class MapTool
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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

        private string fileInput;
        private string fileOutput;
        private int map_Width;
        private int map_Height;
        private int map_FullWidth;
        private int map_FullHeight;
        private string nl = Environment.NewLine;
        private bool fixSectionOrder = false;

        IniConfigSource mapConfig;                                                     // Map file.
        IniConfigSource profileConfig;                                                 // Conversion profile INI file.
        IniConfigSource theaterConfig;                                                 // Theater config INI file.
        List<MapTileContainer> isoMapPack5 = new List<MapTileContainer>();             // Map tile data.
        byte[] overlayPack = null;                                                     // Map overlay ID data.
        byte[] overlayDataPack = null;                                                 // Map overlay frame data.
        string mapTheater = null;                                                      // Map theater data.
        List<string> applicableTheaters = new List<string>();                          // Conversion profile applicable theaters.
        string newTheater = null;                                                      // Conversion profile new theater.
        List<ByteIDConversionRule> tilerules = new List<ByteIDConversionRule>();       // Conversion profile tile rules.
        List<ByteIDConversionRule> overlayrules = new List<ByteIDConversionRule>();    // Conversion profile overlay rules.
        List<StringIDConversionRule> objectrules = new List<StringIDConversionRule>(); // Conversion profile object rules.
        List<SectionConversionRule> sectionrules = new List<SectionConversionRule>();  // Conversion profile section rules.

        public MapTool(string infile, string outfile, string fileconfig = null, string theateriniconfig = null)
        {
            Initialized = false;
            Altered = false;
            fileInput = infile;
            fileOutput = outfile;

            if (!string.IsNullOrEmpty(fileInput) && !string.IsNullOrEmpty(fileOutput))
            {
                try
                {
                    logger.Info("Reading map file '" + infile + "'.");
                    mapConfig = new IniConfigSource(infile);
                    string[] size = mapConfig.Configs["Map"].GetString("Size").Split(',');
                    map_FullWidth = int.Parse(size[2]);
                    map_FullHeight = int.Parse(size[3]);
                    Initialized = parseMapPack();
                    mapTheater = mapConfig.Configs["Map"].GetString("Theater", null);
                    if (mapTheater != null) mapTheater = mapTheater.ToUpper();
                }
                catch (Nini.Ini.IniException e)
                {
                    if (e.Message.Contains("Line"))
                    {
                        string exmsg = e.Message;
                        int linenum;
                        int posnum;
                        int tmp1 = exmsg.IndexOf(",");
                        linenum = Convert.ToInt32(exmsg.Substring(tmp1 - 2, 2));
                        posnum = Convert.ToInt32(exmsg.Substring(tmp1 + 12, 2));
                        logger.Error("Couldn't read map file due to malformed content. Offending character found on line " + linenum.ToString() + ", position " + posnum.ToString() + ".");
                    }
                    else logger.Error("Couldn't read map file. " + e.Message);
                    Initialized = false;
                    return;
                }

                profileConfig = ReadINIFile(fileconfig);
                if (profileConfig == null) Initialized = false;
                else
                {
                    logger.Info("Parsing conversion profile file.");

                    string IncludeFiles = profileConfig.Configs["ProfileData"].Get("IncludeFiles", null);
                    if (!string.IsNullOrEmpty(IncludeFiles))
                    {
                        string[] inc = IncludeFiles.Split(',');
                        string basedir = Path.GetDirectoryName(fileconfig);
                        foreach (string f in inc)
                        {
                            if (File.Exists(basedir +"\\"+ f))
                            {
                                IniConfigSource ic = ReadINIFile(basedir + "\\" + f);
                                if (ic == null) continue;
                                logger.Info("Merging included file '"+f+"' to conversion profile.");
                                profileConfig.Merge(ic);
                            }
                        }
                    }
                    string[] tilerules = null;
                    string[] overlayrules = null;
                    string[] objectrules = null;
                    string[] sectionrules = null;
                    IConfig conf = profileConfig.Configs.GetByName("TileRules");
                    if (conf != null) tilerules = conf.GetValues();
                    conf = profileConfig.Configs.GetByName("OverlayRules");
                    if (conf != null) overlayrules = conf.GetValues();
                    conf = profileConfig.Configs.GetByName("ObjectRules");
                    if (conf != null) objectrules = conf.GetValues();
                    conf = profileConfig.Configs.GetByName("SectionRules");
                    if (conf != null) sectionrules = conf.GetValues();
                    string[] tmp = null;
                    try
                    {
                        newTheater = profileConfig.Configs["TheaterRules"].Get("NewTheater", null);
                        tmp = profileConfig.Configs["TheaterRules"].Get("ApplicableTheaters", null).Split(',');
                    }
                    catch (Exception)
                    {
                    }
                    if (newTheater != null) newTheater = newTheater.ToUpper();
                    if (tmp != null)
                    {
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            applicableTheaters.Add(tmp[i].Trim().ToUpper());
                        }
                    }
                    if (applicableTheaters.Count < 1)
                        applicableTheaters.AddRange(new string[] { "TEMPERATE", "SNOW", "URBAN", "DESERT", "LUNAR", "NEWURBAN" });

                    if (tilerules == null && overlayrules == null && newTheater == null)
                    {
                        logger.Error("No conversion rules to apply in conversion profile file. Aborting.");
                        Initialized = false;
                        return;
                    }

                    parseConfigFile(tilerules, this.tilerules);
                    parseConfigFile(overlayrules, this.overlayrules);
                    parseConfigFile(objectrules, this.objectrules);
                    //parseConfigFile(sectionrules, this.sectionrules);
                }
            }

            else if (!string.IsNullOrEmpty(theateriniconfig))
            {
                try
                {
                    logger.Info("Parsing theater configuration file.");
                    theaterConfig = new IniConfigSource(theateriniconfig);
                }
                catch (Nini.Ini.IniException e)
                {
                    if (e.Message.Contains("Line"))
                    {
                        string exmsg = e.Message;
                        int linenum;
                        int posnum;
                        int tmp1 = exmsg.IndexOf(",");
                        linenum = Convert.ToInt32(exmsg.Substring(tmp1 - 2, 2));
                        posnum = Convert.ToInt32(exmsg.Substring(tmp1 + 12, 2));
                        logger.Error("Couldn't parse theater configuration file due to malformed content. Offending character found on line " + linenum.ToString() + ", position " + posnum.ToString() + ".");
                    }
                    else logger.Error("Couldn't parse theater configuration file. " + e.Message);
                    Initialized = false;
                    return;
                }
            }
            Initialized = true;
        }

        private IniConfigSource ReadINIFile(string fileconfig)
        {
            IniConfigSource ret = null;
            try
            {
                logger.Info("Reading file '" + fileconfig + "'.");
                ret = new IniConfigSource(fileconfig);
            }
            catch (Nini.Ini.IniException e)
            {
                if (e.Message.Contains("Line"))
                {
                    string exmsg = e.Message;
                    int linenum;
                    int posnum;
                    int tmp1 = exmsg.IndexOf(",");
                    linenum = Convert.ToInt32(exmsg.Substring(tmp1 - 2, 2));
                    posnum = Convert.ToInt32(exmsg.Substring(tmp1 + 12, 2));
                    logger.Error("Couldn't read file '"+fileconfig+"' due to malformed content. Offending character found on line " + linenum.ToString() + ", position " + posnum.ToString() + ".");
                }
                else logger.Error("Couldn't read file '" + fileconfig + "'. " + e.Message);
                return null;
            }
            return ret;
        }

        public void ConvertTheaterData()
        {
            if (!Initialized || applicableTheaters == null || newTheater == null) return;
            logger.Info("Attempting to modify theater data of the map file.");
            if (mapTheater != null && !applicableTheaters.Contains(mapTheater))
            {
                logger.Warn("Map theater declaration does not match profile configuration. No modifications will be made to the theater data.");
                return;
            }
            if (newTheater != "" && isValidTheatreName(newTheater))
            {
                mapConfig.Configs["Map"].Set("Theater", newTheater);
                logger.Info("Map theater declaration changed from '" + mapTheater + "' to '" + newTheater + "'.");
                Altered = true;
            }
        }

        public void ConvertTileData()
        {
            if (!Initialized || isoMapPack5.Count < 1 || tilerules == null || tilerules.Count < 1) return;
            else if (mapTheater != null && applicableTheaters != null && !applicableTheaters.Contains(mapTheater)) { logger.Warn("Conversion profile not applicable to maps belonging to this theater. No alterations will be made to the tile data."); return; }
            logger.Info("Attempting to modify tile data of the map file.");
            ApplyTileConversionRules();
        }

        private void ApplyTileConversionRules()
        {
            int cells = (map_Width * 2 - 1) * map_Height;
            int lzoPackSize = cells * 11 + 4;
            byte[] isoMapPack = new byte[lzoPackSize];
            int l, h;
            int i = 0;

            foreach (MapTileContainer t in isoMapPack5)
            {
                if (t.TileIndex < 0 || t.TileIndex == 65535) t.TileIndex = 0;
                foreach (ByteIDConversionRule r in tilerules)
                {
                    l = r.Original_Start;
                    h = r.Original_End;
                    if (t.X == 122 && t.Y == 26)
                        t.UData = 0;
                    if (t.TileIndex >= l && t.TileIndex <= h)
                    {
                        if (r.HeightOverride >= 0)
                        {
                            t.Level = (byte)Math.Min(r.HeightOverride, 14);
                        }
                        if (r.SubIdxOverride >= 0)
                        {
                            t.SubTileIndex = (byte)Math.Min(r.SubIdxOverride, 255);
                        }
                        if (r.New_End == r.New_Start)
                        {
                            logger.Debug("Tile ID " + t.TileIndex + " at " + t.X + "," + t.Y + " changed to " + r.New_Start);
                            t.TileIndex = r.New_Start;
                            break;
                        }
                        else
                        {
                            logger.Debug("Tile ID " + t.TileIndex + " at " + t.X + "," + t.Y + " changed to " + (r.New_Start + Math.Abs(l - t.TileIndex)));
                            t.TileIndex = (r.New_Start + Math.Abs(l - t.TileIndex));
                            break;
                        }
                    }
                }
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
                isoMapPack[i + 10] = t.Level;
                i += 11;
            }
            byte[] lzo = Format5.Encode(isoMapPack, 5);
            string data = Convert.ToBase64String(lzo, Base64FormattingOptions.None);
            overrideBase64MapSection("IsoMapPack5", data);
            Altered = true;
        }

        private void parseConfigFile(string[] new_rules, List<ByteIDConversionRule> current_rules)
        {
            if (new_rules == null || new_rules.Length < 1 || current_rules == null) return;
            current_rules.Clear();
            bool pm1ranged = false;
            bool pm2ranged = false;
            int pm1val1 = 0;
            int pm1val2 = 0;
            int pm2val1 = 0;
            int pm2val2 = 0;
            foreach (string str in new_rules)
            {
                string[] st = str.Split('|');
                if (st.Length < 2) continue;
                if (st[0].Contains('-'))
                {
                    pm1ranged = true;
                    try
                    {
                        string[] st2 = st[0].Split('-');
                        pm1val1 = Convert.ToInt32(st2[0]);
                        pm1val2 = Convert.ToInt32(st2[1]);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                else try
                    {
                        pm1val1 = Convert.ToInt32(st[0]);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                if (st[1].Contains('-'))
                {
                    pm2ranged = true;
                    try
                    {
                        string[] st2 = st[1].Split('-');
                        pm2val1 = Convert.ToInt32(st2[0]);
                        pm2val2 = Convert.ToInt32(st2[1]);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                else try
                    {
                        pm2val1 = Convert.ToInt32(st[1]);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                int heightovr = -1;
                int subovr = -1;
                if (st.Length >= 3 && st[2] != null && !st[2].Equals("*", StringComparison.InvariantCultureIgnoreCase))
                {
                    heightovr = ParseInt(st[2], -1);
                }
                if (st.Length >= 4 && st[3] != null && !st[3].Equals("*", StringComparison.InvariantCultureIgnoreCase))
                {
                    subovr = ParseInt(st[3], -1);
                }

                if (pm1ranged && pm2ranged)
                {
                    current_rules.Add(new ByteIDConversionRule(pm1val1, pm2val1, pm1val2, pm2val2, heightovr, subovr));
                }
                else if (pm1ranged && !pm2ranged)
                {
                    int diff = pm2val1 + (pm1val2 - pm1val1);
                    current_rules.Add(new ByteIDConversionRule(pm1val1, pm2val1, pm1val2, diff, heightovr, subovr));
                }
                else
                {
                    current_rules.Add(new ByteIDConversionRule(pm1val1, pm2val1, -1, -1, heightovr, subovr));
                }
                pm1ranged = false;
                pm2ranged = false;
            }
        }

        private int ParseInt(string s, int defval)
        {
            try
            {
                return Int32.Parse(s);
            }
            catch (Exception)
            {
                return defval;
            }
        }

        private void parseConfigFile(string[] new_rules, List<StringIDConversionRule> current_rules)
        {
            if (new_rules == null || new_rules.Length < 1 || current_rules == null) return;
            current_rules.Clear();
            foreach (string str in new_rules)
            {
                string[] st = str.Split('|');
                if (st.Length == 1) current_rules.Add(new StringIDConversionRule(st[0], null));
                else if (st.Length >= 2) current_rules.Add(new StringIDConversionRule(st[0], st[1]));
            }
        }

        private void parseConfigFile(string[] new_rules, List<SectionConversionRule> current_rules)
        {
            if (new_rules == null || new_rules.Length < 1 || current_rules == null) return;
            current_rules.Clear();
            foreach (string str in new_rules)
            {
                string nid = null;
                int type = 0;
                string[] st = str.Split('|');
                if (st == null || st.Length < 1) continue;
                string id = st[0];
                if (id.StartsWith("+")) { type = 1; id = id.Substring(1, id.Length - 1); }
                else if (id.StartsWith("-")) { type = 2; id = id.Substring(1, id.Length - 1); }
                else if (id.StartsWith("*"))
                {
                    string[] ids = id.Split(',');
                    if (ids.Length > 1)
                    {
                        id = ids[0].Replace("*", "");
                        nid = ids[1];
                    }
                    else id = id.Replace("*", "");
                }
                List<SectionKVP> kvplist = new List<SectionKVP>();
                for (int i = 1; i < st.Length; i++)
                {
                    string[] kvp = st[i].Split('=');
                    int kvptype = 0;
                    if (kvp.Length < 1) continue;
                    string key = kvp[0];
                    string value = null;
                    if (kvp.Length == 2) value = kvp[1];
                    if (key.StartsWith("+")) { kvptype = 1; key = key.Substring(1, key.Length - 1); }
                    else if (key.StartsWith("-")) { kvptype = 2; key = key.Substring(1, key.Length - 1); }
                    kvplist.Add(new SectionKVP(key, value, (SectionRuleType)kvptype));
                }
                current_rules.Add(new SectionConversionRule(id, nid, kvplist, (SectionRuleType)type));
            }
        }

        private bool parseMapPack()
        {
            logger.Info("Parsing IsoMapPack5.");
            string data = "";
            string[] tmp = mapConfig.Configs["IsoMapPack5"].GetValues();
            foreach (string str in tmp)
            {
                data += str;
            }
            int cells;
            byte[] isoMapPack;
            try
            {
                string size = mapConfig.Configs["Map"].Get("Size");
                string[] st = size.Split(',');
                map_Width = Convert.ToInt16(st[2]);
                map_Height = Convert.ToInt16(st[3]);
                byte[] lzoData = Convert.FromBase64String(data);
                byte[] test = lzoData;
                cells = (map_Width * 2 - 1) * map_Height;
                int lzoPackSize = cells * 11 + 4;
                isoMapPack = new byte[lzoPackSize];
                uint total_decompress_size = Format5.DecodeInto(lzoData, isoMapPack);
            }
            catch (Exception)
            {
                return false;
            }
            MemoryFile mf = new MemoryFile(isoMapPack);
            for (int i = 0; i < cells; i++)
            {
                ushort rx = mf.ReadUInt16();
                ushort ry = mf.ReadUInt16();
                int tilenum = mf.ReadInt32();
                byte subtile = mf.ReadByte();
                byte level = mf.ReadByte();
                byte udata = mf.ReadByte();
                int dx = rx - ry + map_FullWidth - 1;
                int dy = rx + ry - map_FullWidth - 1;
                isoMapPack5.Add(new MapTileContainer((short)rx, (short)ry, tilenum, subtile, level, udata));
            }
            return true;
        }

        public void ConvertObjectData()
        {
            if (!Initialized || overlayrules == null || objectrules.Count < 1) return;
            else if (mapTheater != null && applicableTheaters != null && !applicableTheaters.Contains(mapTheater)) { logger.Warn("Conversion profile not applicable to maps belonging to this theater. No alterations will be made to the object data."); return; }
            logger.Info("Attempting to modify object data of the map file.");
            ApplyObjectConversionRules("Aircraft");
            ApplyObjectConversionRules("Units");
            ApplyObjectConversionRules("Infantry");
            ApplyObjectConversionRules("Structures");
        }

        private void ApplyObjectConversionRules(string sectionname)
        {
            if (string.IsNullOrEmpty(sectionname)) return;
            IConfig section = mapConfig.Configs[sectionname];
            if (section == null) return;
            string[] values = section.GetValues();
            string[] keys = section.GetKeys();
            for (int i = 0; i < Math.Min(values.Length, keys.Length); i++)
            {
                if (string.IsNullOrEmpty(values[i])) continue;
                foreach (StringIDConversionRule rule in objectrules)
                {
                    if (rule == null || rule.Original == null) continue;
                    if (values[i].Contains(rule.Original))
                    {
                        if (rule.New == null)
                        {
                            logger.Debug("Removed " + sectionname + " object with ID '" + rule.Original + "' from the map file.");
                            section.Remove(keys[i]);
                            Altered = true;
                        }
                        else
                        {
                            logger.Debug("Replaced " + sectionname + " object with ID '" + rule.Original + "' with object of ID '" + rule.New + "'.");
                            section.Set(keys[i], values[i].Replace("," + rule.Original + ",", "," + rule.New + ","));
                            Altered = true;
                        }
                    }
                }
            }
        }

        public void ConvertSectionData()
        {
            if (!Initialized || overlayrules == null || sectionrules.Count < 1) return;
            else if (mapTheater != null && applicableTheaters != null && !applicableTheaters.Contains(mapTheater)) { logger.Warn("Conversion profile not applicable to maps belonging to this theater. No alterations will be made to the section data."); return; }
            logger.Info("Attempting to modify section data of the map file.");
            ApplySectionConversionRules();
            fixSectionOrder = true;
        }

        private void ApplySectionConversionRules()
        {
            foreach (SectionConversionRule rule in sectionrules)
            {
                IConfig section = mapConfig.Configs[rule.SectionID];
                if (section != null && rule.Type == SectionRuleType.Remove) { logger.Debug("Removed section '" + section.Name + "'."); mapConfig.Configs.Remove(section); Altered = true; continue; }
                else if (section != null && rule.Type == SectionRuleType.Add) { logger.Debug("Added section '" + rule.SectionID + "'."); mapConfig.Configs.Add(rule.SectionID); Altered = true; section = mapConfig.Configs[rule.SectionID]; }
                else if (section == null) continue;
                foreach (SectionKVP kvp in rule.KVPList)
                {
                    if (kvp.Type == SectionRuleType.Remove) { logger.Debug("Removed key '" + kvp.Key + "' from section '" + section.Name + "."); section.Remove(kvp.Key); Altered = true; continue; }
                    else { section.Set(kvp.Key, kvp.Value); logger.Debug("Set section '" + section.Name + "' key '" + kvp.Key + "' value to '" + kvp.Value + "'."); Altered = true; continue; }
                }
            }
        }

        public void ConvertOverlayData()
        {
            if (!Initialized || overlayrules == null || overlayrules.Count < 1) return;
            else if (mapTheater != null && applicableTheaters != null && !applicableTheaters.Contains(mapTheater)) { logger.Warn("Conversion profile not applicable to maps belonging to this theater. No alterations will be made to the overlay data."); return; }

            parseOverlayPack();

            logger.Info("Attempting to modify overlay data of the map file.");
            ApplyOverlayConversionRules();
        }

        private void ApplyOverlayConversionRules()
        {
            for (int i = 0; i < Math.Min(overlayPack.Length, overlayDataPack.Length); i++)
            {
                if (overlayPack[i] == 255) continue;
                if (overlayPack[i] < 0 || overlayPack[i] > 255) overlayPack[i] = 0;
                if (overlayDataPack[i] < 0 || overlayDataPack[i] > 255) overlayDataPack[i] = 0;
                foreach (ByteIDConversionRule rule in overlayrules)
                {
                    if (!rule.ValidForOverlays()) continue;
                    if (overlayPack[i] >= rule.Original_Start && overlayPack[i] <= rule.Original_End)
                    {
                        if (rule.New_End == rule.New_Start)
                        {
                            logger.Debug("Overlay ID '" + overlayPack[i] + " at array slot " + i + "' changed to '" + rule.New_Start + "'.");
                            overlayPack[i] = (byte)rule.New_Start;
                            break;
                        }
                        else
                        {
                            logger.Debug("Overlay ID '" + overlayPack[i] + " at array slot " + i + "' changed to '" + (rule.New_Start + Math.Abs(rule.Original_Start - overlayPack[i])) + "'.");
                            overlayPack[i] = (byte)(rule.New_Start + Math.Abs(rule.Original_Start - overlayPack[i]));
                            break;
                        }
                    }
                }
            }
            saveOverlayPack();
        }

        private void parseOverlayPack()
        {
            logger.Info("Parsing OverlayPack.");
            IConfig conf = mapConfig.Configs["OverlayPack"];
            if (conf == null) return;
            byte[] format80Data = Convert.FromBase64String(concatenatedValuesFromSection(conf));
            var overlayPack = new byte[1 << 18];
            Format5.DecodeInto(format80Data, overlayPack, 80);

            logger.Info("Parsing OverlayDataPack.");
            conf = mapConfig.Configs["OverlayDataPack"];
            if (conf == null) return;
            format80Data = Convert.FromBase64String(concatenatedValuesFromSection(conf));
            var overlayDataPack = new byte[1 << 18];
            Format5.DecodeInto(format80Data, overlayDataPack, 80);

            this.overlayPack = overlayPack;
            this.overlayDataPack = overlayDataPack;
        }

        private void saveOverlayPack()
        {
            string base64_overlayPack = Convert.ToBase64String(Format5.Encode(overlayPack, 80), Base64FormattingOptions.None);
            string base64_overlayDataPack = Convert.ToBase64String(Format5.Encode(overlayDataPack, 80), Base64FormattingOptions.None);
            overrideBase64MapSection("OverlayPack", base64_overlayPack);
            overrideBase64MapSection("OverlayDataPack", base64_overlayDataPack);
            Altered = true;
        }

        private void overrideBase64MapSection(string sectionName, string data)
        {
            mapConfig.RemoveConfig(sectionName);
            mapConfig.AddConfig(sectionName);
            int lx = 70;
            int rownum = 1;
            for (int x = 0; x < data.Length; x += lx)
            {
                mapConfig.Configs[sectionName].Set(rownum++.ToString(CultureInfo.InvariantCulture), data.Substring(x, Math.Min(lx, data.Length - x)));
            }
        }

        private string concatenatedValuesFromSection(IConfig section)
        {
            if (section == null) return null;
            var sb = new StringBuilder();
            string[] vals = section.GetValues();
            foreach (string v in vals)
                sb.Append(v);
            return sb.ToString();
        }


        public void ListTileSetData()
        {
            if (theaterConfig == null) return;

            TilesetCollection mtiles = TilesetCollection.ParseFromINIFile(theaterConfig);

            if (mtiles == null || mtiles.Count < 1) { logger.Error("Could not parse tileset data from theater configuration file '" + theaterConfig.SavePath + "'."); return; };

            logger.Info("Attempting to list tileset data for a theater based on file: '" + theaterConfig.SavePath + "'.");

            string print = "";
            int tilecounter = 0;
            print += "Theater tileset data gathered from file '" + theaterConfig.SavePath + "'." + nl + nl;
            foreach (Tileset ts in mtiles)
            {
                if (ts.TilesInSet < 1)
                {
                    logger.Debug(ts.SetID + " (" + ts.SetName + ")" + " skipped due to tile count of 0.");
                    continue;
                }
                print += ts.getPrintableData(ref tilecounter);
                logger.Debug(ts.SetID + " (" + ts.SetName + ")" + " added to the list.");
            }
            WriteTextFile(print, fileOutput);
        }

        private void WriteTextFile(string content, string filename)
        {
            TextWriter tw;
            try
            {
                tw = new StreamWriter(filename, false);
                logger.Info("Writing tileset data into file '" + filename + "'.");
                tw.Write(content);

            }
            catch (Exception)
            {
                logger.Error("Could not write text file '" + filename + "'.");
                return;
            }
            tw.Close();
        }

        public void Save()
        {
            if (!fixSectionOrder) mapConfig.Save(fileOutput);
            else
            {
                IConfig digest = mapConfig.Configs["Digest"];
                IConfig basic = mapConfig.Configs["Basic"];
                if (digest != null) mapConfig.RemoveConfig("Digest");
                else { mapConfig.Save(fileOutput); return; };
                if (basic != null) mapConfig.RemoveConfig("Basic");
                string tmp = Path.GetTempFileName();
                mapConfig.Save(tmp);
                IniConfigSource map_tmp = new IniConfigSource(tmp);
                mapConfig = new IniConfigSource();
                addHardCopySection(mapConfig.Configs, basic);
                foreach (IConfig config in map_tmp.Configs)
                {
                    addHardCopySection(mapConfig.Configs, config);
                }
                addHardCopySection(mapConfig.Configs, digest);
                mapConfig.Save(fileOutput);
            }
        }

        private void addHardCopySection(ConfigCollection configCollection, IConfig section)
        {
            configCollection.Add(section.Name);
            IConfig newsection = configCollection[section.Name];
            string[] keys = section.GetKeys();
            string[] values = section.GetValues();
            for (int i = 0; i < Math.Min(keys.Length, values.Length); i++)
            {
                newsection.Set(keys[i], values[i]);
            }
        }

        public static bool isValidTheatreName(string tname)
        {
            if (tname == "TEMPERATE" || tname == "SNOW" || tname == "LUNAR" || tname == "DESERT" || tname == "URBAN" || tname == "NEWURBAN") return true;
            return false;
        }

        public static short shortFromBytes(byte b1, byte b2)
        {
            byte[] b = new byte[] { b2, b1 };
            return BitConverter.ToInt16(b, 0);
        }

        public static int IntFromBytes(byte b1, byte b2, byte b3, byte b4)
        {
            byte[] b = new byte[] { b1, b2, b3, b4 };
            return BitConverter.ToInt32(b, 0);
        }

    }
}
