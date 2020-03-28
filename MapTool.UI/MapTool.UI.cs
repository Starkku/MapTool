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
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using StarkkuUtils.FileTypes;
using StarkkuUtils.Utilities;

namespace MapTool.UI
{
    public partial class MapToolUI : Form
    {
        private readonly List<string> ValidMapExts = new List<string> { ".map", ".mpr", ".yrm" };
        private readonly string MapToolExecutable = "MapTool.exe";
        private readonly string ProfileDirectory = AppDomain.CurrentDomain.BaseDirectory + "Profiles";

        private int HoverIndex = -1;
        private List<ListBoxProfile> Profiles = new List<ListBoxProfile>();
        private ListBoxProfile SelectedProfile = null;

        private bool EnableWriteDebugLog = false;

        public MapToolUI(string[] args)
        {
            ParseArguments(args);
            CheckExe();
            LoadProfiles();
            InitializeComponent();
            listProfiles.DataSource = Profiles;
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            Text += " v." + v.ToString();
            if (Profiles.Count > 0 && listProfiles.SelectedIndex != -1) buttonEditProfile.Enabled = true;
            string ext1 = "", ext2 = "", delim = "", delim2 = "";
            for (int i = 0; i < ValidMapExts.Count; i++)
            {
                if (i == 0) { delim = ""; delim2 = ""; }
                else { delim = ","; delim2 = ";"; }
                ext1 += delim + "*" + ValidMapExts[i];
                ext2 += delim2 + "*" + ValidMapExts[i];
            }
            openFileDialog.Filter = "Map files (" + ext1 + ")|" + ext2;
            if (EnableWriteDebugLog)
            {
                string logfile = AppDomain.CurrentDomain.BaseDirectory + Path.ChangeExtension(AppDomain.CurrentDomain.FriendlyName, ".log");
                Logger.Initialize(logfile, true, false);
            }
        }

        private void ParseArguments(string[] args)
        {
            foreach (string arg in args)
            {
                switch (arg.ToLower())
                {
                    case "-log":
                        EnableWriteDebugLog = true;
                        continue;
                    default:
                        continue;
                }
            }
        }

        private void CheckExe()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + MapToolExecutable))
            {
                MessageBox.Show("Could not find the map tool executable (" + MapToolExecutable + ") in the program directory. Aborting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                Close();
                Environment.Exit(1);
            }
        }

        private void LoadProfiles()
        {
            string[] files = new string[0];
            if (!Directory.Exists(ProfileDirectory))
            {
                MessageBox.Show("Could not find the profile directory (sub-directory called 'Profiles' in the program directory). Aborting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                Close();
                Environment.Exit(1);
            }
            else
            {
                files = Directory.GetFiles(ProfileDirectory, "*.ini", SearchOption.TopDirectoryOnly);
                if (files.Length < 1)
                {
                    MessageBox.Show("Could not find any conversion profiles in the profile directory (sub-directory called 'Profiles' in the program directory). Aborting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    Close();
                    Environment.Exit(1);
                }
            }
            foreach (string s in files)
            {
                try
                {
                    INIFile profile = new INIFile(s);
                    if (!profile.SectionExists("ProfileData")) continue;
                    Profiles.Add(new ListBoxProfile(s, profile.GetKey("ProfileData", "Name", Path.GetFileName(s)), profile.GetKey("ProfileData", "Description", "Description Not Available")));
                }
                catch (Exception)
                {
                    continue;
                }
            }
            Profiles.Sort();
        }

        // Check if the filename is already on the list.
        private bool CheckIfDuplicate(string filename)
        {
            foreach (ListBoxFile f in listFiles.Items)
            {
                if (f.FileName.Equals(filename)) return true;
            }
            return false;
        }

        private void DeleteSelectedItems()
        {
            for (int i = listFiles.SelectedIndices.Count - 1; i >= 0; i--)
            {
                listFiles.Items.RemoveAt(listFiles.SelectedIndices[i]);
            }
            if (listFiles.Items.Count < 1 || SelectedProfile == null) buttonConvert.Enabled = false;
            if (listFiles.Items.Count < 1) buttonSelect.Enabled = false;
        }

        private void AddMapFiles(string[] filenames)
        {
            ListBoxFile file;
            foreach (string filename in filenames)
            {
                string ext = Path.GetExtension(filename);
                if (!ValidMapExts.Contains(ext)) continue;
                if (CheckIfDuplicate(filename)) continue;
                file = new ListBoxFile(filename, Path.GetFileName(filename), filename);
                listFiles.Items.Add(file);
            }
            if (listFiles.Items.Count > 0 && SelectedProfile != null) buttonConvert.Enabled = true;
            if (listFiles.Items.Count > 0) buttonSelect.Enabled = true;
        }

        // Drag & drop stuff.
        private void ListFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
            else e.Effect = DragDropEffects.None;
        }

        // Add drag & dropped files into the list.
        private void ListFiles_DragDrop(object sender, DragEventArgs e)
        {
            List<string> filenames = new List<string>();

            foreach (string s in (string[])e.Data.GetData(DataFormats.FileDrop, false))
            {
                if (Directory.Exists(s))
                {
                    filenames.AddRange(Directory.GetFiles(s));
                }
                else
                {
                    filenames.Add(s);
                }
            }
            AddMapFiles(filenames.ToArray());
        }

        // Show tooltips for the items displaying the full file path.
        private void ListFiles_MouseMove(object sender, MouseEventArgs e)
        {
            int newhoveridx = listFiles.IndexFromPoint(e.Location);

            if (HoverIndex != newhoveridx)
            {
                HoverIndex = newhoveridx;
                if (HoverIndex > -1)
                {
                    toolTip.Active = false;
                    ListBoxFile f = listFiles.Items[HoverIndex] as ListBoxFile;
                    toolTip.SetToolTip(listFiles, f.Tooltip);
                    toolTip.Active = true;
                }
            }
        }

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            DeleteSelectedItems();
        }

        private void ListFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedItems();
            }
        }

        private void ListFiles_SelectedValueChanged(object sender, EventArgs e)
        {
            if (listFiles.SelectedIndices.Count > 0) buttonRemove.Enabled = true;
            else buttonRemove.Enabled = false;
        }

        private void ButtonConvert_Click(object sender, EventArgs e)
        {
            textBoxLogger.Text = "";
            AppendToLog("Processing maps.\r\n");
            tabControl.SelectedIndex = 1;
            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                ToggleControlState(false);
                foreach (ListBoxFile f in listFiles.Items)
                {
                    ProcessMap(f.FileName);
                }
                ToggleControlState(true);
                AppendToLog("Processed all maps.");
                AppendToLog("");
            });
        }
        private void ProcessMap(string filename)
        {
            string outputfilename = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + "_altered" + Path.GetExtension(filename);
            if (cbOverwrite.Checked) outputfilename = filename;
            string extra = "";
            //if (EnableWriteDebugLog) extra += " -log";
            string cmd = "-i=\"" + filename + "\" -o=\"" + outputfilename + "\" -p=\"" + SelectedProfile.FileName + "\"" + extra;

            try
            {
                var p = new Process { StartInfo = { FileName = AppDomain.CurrentDomain.BaseDirectory + MapToolExecutable, Arguments = cmd } };

                // Catch the command line output to display in log.
                p.OutputDataReceived += ConsoleDataReceived;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();

                p.BeginOutputReadLine();
                p.WaitForExit();
                if (p.ExitCode == 0)
                {
                    AppendToLog("");
                    AppendToLog("Successfully finished processing map '" + filename + "'.");
                }
                else
                {
                    AppendToLog("");
                    AppendToLog("Processing on map '" + filename + "' failed.");
                }
                AppendToLog("");
            }
            catch (Exception e)
            {
                AppendToLog("Error encountered. Message: " + e.Message);
            }
        }
        private void ConsoleDataReceived(object sender, DataReceivedEventArgs e)
        {
            AppendToLog(e.Data);
        }

        private delegate void LogDelegate(string s);
        private void AppendToLog(string s)
        {
            if (s == null) return;
            if (InvokeRequired)
            {
                Invoke(new LogDelegate(AppendToLog), s);
                return;
            }
            textBoxLogger.AppendText(s + "\r\n");
            if (EnableWriteDebugLog)
            {
                Logger.LogToFileOnly(s);
            }
        }

        private delegate void ControlStateDelegate(bool enable);
        private void ToggleControlState(bool enable)
        {
            if (InvokeRequired)
            {
                Invoke(new ControlStateDelegate(ToggleControlState), enable);
                return;
            }
            listFiles.Enabled = enable;
            listProfiles.Enabled = enable;
            buttonBrowse.Enabled = enable;
            buttonConvert.Enabled = enable;
            buttonSelect.Enabled = enable;
            buttonRemove.Enabled = enable;
        }


        private void ListProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                SelectedProfile = Profiles[listProfiles.SelectedIndex];
                labelProfileDescription.Text = SelectedProfile.Description;
            }
            catch (Exception)
            {

                SelectedProfile = null;
            }
            if (SelectedProfile == null || listFiles.Items.Count < 1) buttonConvert.Enabled = false;
        }

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                AddMapFiles(openFileDialog.FileNames);
            }
        }

        private void ButtonSelect_Click(object sender, EventArgs e)
        {
            if (listFiles.SelectedIndices.Count > 0)
            {
                listFiles.SelectedIndex = -1;
            }
            else
            {
                for (int i = 0; i < listFiles.Items.Count; i++)
                {
                    listFiles.SetSelected(i, true);
                }
            }
        }

        private void ButtonEditProfile_Click(object sender, EventArgs e)
        {
            Process.Start(SelectedProfile.FileName);
        }

        private void MapFixer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D && e.Modifiers == Keys.Alt)
            {
            }

        }

        private void LinkLabelAboutGithub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/Starkku/MapTool");
        }

        private void LinkLabelAboutOpenRA_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/OpenRA/OpenRA");
        }

        private void LinkLabelRenderer_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/zzattack/ccmaps-net");
        }
    }

    class ListBoxFile
    {
        public string FileName { get; set; }
        public string DisplayName { get; set; }
        public string Tooltip { get; set; }

        public ListBoxFile(string filename, string displayname, string tooltip)
        {
            FileName = filename;
            DisplayName = displayname;
            Tooltip = tooltip;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

    class ListBoxProfile : IComparable<ListBoxProfile>
    {
        public string FileName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ListBoxProfile(string filename, string name, string description)
        {
            FileName = filename;
            Name = name;
            Description = description;
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(ListBoxProfile other)
        {
            return Name.CompareTo(other.Name);
        }
    }
}
