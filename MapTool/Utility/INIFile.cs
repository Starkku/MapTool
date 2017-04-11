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
using System.IO;
using System.Text;

namespace MapTool.Utility
{
    class INISection
    {
        internal string Name = null;
        internal List<INIKeyValuePair> Lines = null;
    }

    class INIKeyValuePair
    {
        internal string Key = null;
        internal string Value = null;
        public INIKeyValuePair(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    public class INIFile
    {
        private string fname = null;
        private List<INISection> Sections = null;

        public string Filename
        {
            get { return fname; }
            set { fname = value; }
        }

        public bool Initialized { get; set; }

        public INIFile(string fname)
        {
            Filename = fname;
            if (String.IsNullOrEmpty(Filename)) return;
            if (!File.Exists(Filename)) return;
            LoadAndParse();
            Initialized = true;
        }

        private void LoadAndParse()
        {
            string[] lines = File.ReadAllLines(Filename);
            INISection current = null;
            Sections = new List<INISection>();
            foreach (string line in lines)
            {
                string tmp = line.Trim();
                if (tmp.StartsWith(";")) continue;
                if (String.IsNullOrEmpty(tmp)) continue;
                if (tmp.StartsWith("[") && tmp.Contains("]"))
                {
                    current = new INISection();
                    int st = tmp.IndexOf('[') + 1;
                    int end = tmp.IndexOf(']') - 1;
                    current.Name = tmp.Substring(st, end);
                    current.Lines = new List<INIKeyValuePair>();
                    Sections.Add(current);
                }
                else
                {
                    INIKeyValuePair kvp = GetKVP(line.Trim());
                    if (kvp == null) continue;
                    current.Lines.Add(kvp);
                }
            }
        }

        private INIKeyValuePair GetKVP(string line)
        {
            string key = null;
            string value = null;
            if (line.Contains("="))
            {
                key = line.Substring(0, line.IndexOf('=')).Trim();
                value = wipeComments(line.Substring(line.IndexOf('=') + 1, line.Length - line.IndexOf('=') - 1)).Trim();
            }
            else
            {
                value = wipeComments(line).Trim();
            }
            INIKeyValuePair kvp = new INIKeyValuePair(key, value);
            return kvp;
        }

        private string wipeComments(string line)
        {
            if (line.Contains(";"))
            {
                return line.Substring(0, line.IndexOf(';')).Trim();
            }
            return line;
        }

        public string GetKey(string Section, string Key, string DefaultValue)
        {
            if (!Initialized) return null;
            INISection sec = Sections.Find(i => i.Name == Section);
            if (sec == null) return DefaultValue;
            INIKeyValuePair kvp = sec.Lines.Find(i => i.Key == Key);
            if (kvp == null) return null;
            return kvp.Value;
        }

        public void SetKey(string Section, string Key, string Value)
        {
            if (!Initialized || Sections == null) return;
            INISection sec = Sections.Find(i => i.Name == Section);
            if (sec == null) AddSection(Section);
            INIKeyValuePair kvp = sec.Lines.Find(i => i.Key == Key);
            if (kvp == null)
            {
                sec.Lines.Add(new INIKeyValuePair(Key, Value));
            }
            else kvp.Value = Value;
        }

        public void RemoveKey(string Section, string Key)
        {
            if (!Initialized || Sections == null) return;
            INISection sec = Sections.Find(i => i.Name == Section);
            if (sec == null) return;
            INIKeyValuePair kvp = sec.Lines.Find(i => i.Key == Key);
            sec.Lines.Remove(kvp);
        }

        public bool SectionExists(string Section)
        {
            if (!Initialized || Sections == null) return false;
            INISection sec = Sections.Find(i => i.Name == Section);
            if (sec == null) return false;
            return true;
        }

        public string[] GetValues(string Section)
        {
            if (!Initialized || Sections == null) return null;
            INISection sec = Sections.Find(i => i.Name == Section);
            if (sec == null) return null;
            string[] values = new string[sec.Lines.Count];
            int c = 0;
            foreach (INIKeyValuePair kvp in sec.Lines)
            {
                values[c++] = kvp.Value;
            }
            return values;
        }

        public KeyValuePair<string, string>[] GetKeyValuePairs(string Section)
        {
            if (!Initialized || Sections == null) return null;
            INISection sec = Sections.Find(i => i.Name == Section);
            if (sec == null) return null;
            KeyValuePair<string, string>[] kvps = new KeyValuePair<string, string>[sec.Lines.Count];
            int c = 0;
            foreach (INIKeyValuePair kvp in sec.Lines)
            {
                kvps[c++] = new KeyValuePair<string, string>(kvp.Key, kvp.Value);
            }
            return kvps;
        }

        public void Merge(INIFile mergeFile)
        {
            if (!Initialized || !mergeFile.Initialized || mergeFile.Sections == null || mergeFile.Sections.Count < 1 || Sections == null) return;
            foreach (INISection sec in mergeFile.Sections)
            {
                if (sec == null) continue;
                INISection f_sec = Sections.Find(i => i.Name == sec.Name);
                if (f_sec == null)
                {
                    f_sec = new INISection();
                    f_sec.Name = sec.Name;
                    f_sec.Lines = new List<INIKeyValuePair>();
                    Sections.Add(f_sec);
                }
                foreach (INIKeyValuePair kvp in sec.Lines)
                {
                    INIKeyValuePair f_kvp = null;
                    if (kvp.Key != null)
                    {
                        f_kvp = f_sec.Lines.Find(i => i.Key == kvp.Key);
                    }
                    else
                    {
                        f_kvp = f_sec.Lines.Find(i => i.Value == kvp.Value);
                    }
                    if (f_kvp != null) f_kvp.Value = kvp.Value;
                    else f_sec.Lines.Add(new INIKeyValuePair(kvp.Key, kvp.Value));
                }
            }
        }

        public void RemoveSection(string Section)
        {
            if (!Initialized || Sections == null) return;
            INISection sec = Sections.Find(i => i.Name == Section);
            if (sec == null) return;
            Sections.Remove(sec);
        }

        public void AddSection(string Section)
        {
            if (!Initialized || Sections == null) return;
            INISection sec = new INISection();
            sec.Name = Section;
            sec.Lines = new List<INIKeyValuePair>();
            Sections.Add(sec);
        }

        public string[] GetSections()
        {
            if (!Initialized || Sections == null) return null;
            string[] sections = new string[Sections.Count];
            int c = 0;
            foreach (INISection sec in Sections)
            {
                sections[c++] = sec.Name;
            }
            return sections;
        }

        public void ReplaceSectionValues(string Section, List<string> Lines)
        {
            if (!Initialized || Sections == null) return;
            INISection sec = Sections.Find(i => i.Name == Section);
            if (sec == null) AddSection(Section);
            sec.Lines.Clear();
            int c = 0;
            foreach (string line in Lines)
            {
                sec.Lines.Add(new INIKeyValuePair(c++.ToString(), line));
            }
        }

        public void Save(string filenameOutput)
        {
            if (!Initialized || Sections == null) return;
            if (String.IsNullOrEmpty(filenameOutput)) filenameOutput = Filename;
            List<string> lines = new List<string>();
            foreach (INISection sec in Sections)
            {
                lines.Add("[" + sec.Name + "]");
                foreach (INIKeyValuePair kvp in sec.Lines)
                {
                    if (kvp.Key == null) lines.Add(kvp.Value);
                    else lines.Add(kvp.Key + "=" + kvp.Value);
                }
                lines.Add("");
            }
            try
            {
                File.WriteAllLines(filenameOutput, lines.ToArray());
            }
            catch (Exception)
            {
            }
        }
    }
}