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
using Nini.Config;
using System.Diagnostics;
using System.Threading;

namespace MapTool_UI
{
    public partial class MapTool_UI : Form
    {
        private readonly List<string> ValidMapExts = new List<string> { ".map", ".mpr", ".yrm" };
        private readonly string MapToolExecutable = "MapTool.exe";
        private readonly string ProfileDirectory = AppDomain.CurrentDomain.BaseDirectory + "Profiles";

        private int hoveridx = -1;
        private List<ListBoxProfile> profiles = new List<ListBoxProfile>();
        private ListBoxProfile selectedprofile = null;

        public MapTool_UI()
        {
            checkExe();
            loadProfiles();
            InitializeComponent();
            listProfiles.DataSource = profiles;
            Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Text += " v." + v.ToString();
            if (profiles.Count > 0 && listProfiles.SelectedIndex != -1) buttonEditProfile.Enabled = true;
            string ext1 = "", ext2 = "", delim = "", delim2 = "";
            for (int i = 0; i < ValidMapExts.Count; i++)
            {
                if (i == 0) { delim = ""; delim2 = ""; }
                else { delim = ","; delim2 = ";"; }
                ext1 += delim + "*" + ValidMapExts[i];
                ext2 += delim2 + "*" + ValidMapExts[i];
            }
            openFileDialog.Filter = "Map files (" + ext1 + ")|" + ext2;
        }

        private void checkExe()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + MapToolExecutable))
            {
                MessageBox.Show("Could not find the map tool executable in the program directory. Aborting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                Close();
                Environment.Exit(1);
            }
        }

        private void loadProfiles()
        {
            if (!Directory.Exists(ProfileDirectory))
            {
                MessageBox.Show("Could not find the profile directory (sub-directory called 'Profiles' in the program directory). Aborting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                Close();
                Environment.Exit(1);
            }

            string[] files = Directory.GetFiles(ProfileDirectory, "*.ini", SearchOption.TopDirectoryOnly);
            foreach (string s in files)
            {
                try
                {
                    IniConfigSource profile = new IniConfigSource(s);
                    profiles.Add(new ListBoxProfile(s, profile.Configs["ProfileData"].GetString("Name", s), profile.Configs["ProfileData"].GetString("Description", "N/A")));
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        // Check if the filename is already on the list.
        private bool checkIfDuplicate(string filename)
        {
            foreach (ListBoxFile f in listFiles.Items)
            {
                if (f.FileName.Equals(filename)) return true;
            }
            return false;
        }

        private void deleteSelectedItems()
        {
            for (int i = listFiles.SelectedIndices.Count - 1; i >= 0; i--)
            {
                listFiles.Items.RemoveAt(listFiles.SelectedIndices[i]);
            }
            if (listFiles.Items.Count < 1 || selectedprofile == null) buttonConvert.Enabled = false;
            if (listFiles.Items.Count < 1) buttonSelect.Enabled = false;
        }

        private void addMapFiles(string[] filenames)
        {
            ListBoxFile file;
            foreach (string filename in filenames)
            {
                string ext = Path.GetExtension(filename);
                if (!ValidMapExts.Contains(ext)) continue;
                if (checkIfDuplicate(filename)) continue;
                file = new ListBoxFile(filename, Path.GetFileName(filename), filename);
                listFiles.Items.Add(file);
            }
            if (listFiles.Items.Count > 0 && selectedprofile != null) buttonConvert.Enabled = true;
            if (listFiles.Items.Count > 0) buttonSelect.Enabled = true;
        }

        // Drag & drop stuff.
        private void listFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
            else e.Effect = DragDropEffects.None;
        }

        // Add drag & dropped files into the list.
        private void listFiles_DragDrop(object sender, DragEventArgs e)
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
            addMapFiles(filenames.ToArray());
        }

        // Show tooltips for the items displaying the full file path.
        private void listFiles_MouseMove(object sender, MouseEventArgs e)
        {
            int newhoveridx = listFiles.IndexFromPoint(e.Location);

            if (hoveridx != newhoveridx)
            {
                hoveridx = newhoveridx;
                if (hoveridx > -1)
                {
                    toolTip.Active = false;
                    ListBoxFile f = listFiles.Items[hoveridx] as ListBoxFile;
                    toolTip.SetToolTip(listFiles, f.Tooltip);
                    toolTip.Active = true;
                }
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            deleteSelectedItems();
        }

        private void listFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                deleteSelectedItems();
            }
        }

        private void listFiles_SelectedValueChanged(object sender, EventArgs e)
        {
            if (listFiles.SelectedIndices.Count > 0) buttonRemove.Enabled = true;
            else buttonRemove.Enabled = false;
        }

        private void buttonConvert_Click(object sender, EventArgs e)
        {
            appendToLog("Processing maps.\r\n");
            tabControl.SelectedIndex = 1;
            foreach (ListBoxFile f in listFiles.Items)
            {
                processMap(f.FileName);
            }
        }
        private void processMap(string filename)
        {
            string outputfilename = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + "_altered" + Path.GetExtension(filename);
            if (cbOverwrite.Checked) outputfilename = filename;
            string cmd = "-i=\"" + filename + "\" -o=\"" + outputfilename + "\" -p=\"" + selectedprofile.FileName + "\" -c";
            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                try
                {
                    toggleControlState(false);
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
                        appendToLog("Successfully finished processing map '" + filename + "'.");
                    else
                        appendToLog("Processing on map '" + filename + "' failed.");
                    toggleControlState(true);
                }
                catch (Exception)
                {
                }
            });
        }
        private void ConsoleDataReceived(object sender, DataReceivedEventArgs e)
        {
            appendToLog(e.Data);
        }

        private delegate void LogDelegate(string s);
        private void appendToLog(string s)
        {
            if (InvokeRequired)
            {
                Invoke(new LogDelegate(appendToLog), s);
                return;
            }
            textBoxLogger.AppendText(s + "\r\n");
        }

        private delegate void ControlStateDelegate(bool enable);
        private void toggleControlState(bool enable)
        {
            if (InvokeRequired)
            {
                Invoke(new ControlStateDelegate(toggleControlState), enable);
                return;
            }
            listFiles.Enabled = enable;
            listProfiles.Enabled = enable;
            buttonBrowse.Enabled = enable;
            buttonConvert.Enabled = enable;
            buttonSelect.Enabled = enable;
            buttonRemove.Enabled = enable;
        }


        private void listProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                selectedprofile = profiles[listProfiles.SelectedIndex];
                labelProfileDescription.Text = selectedprofile.Description;
            }
            catch (Exception)
            {

                selectedprofile = null;
            }
            if (selectedprofile == null || listFiles.Items.Count < 1) buttonConvert.Enabled = false;
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                addMapFiles(openFileDialog.FileNames);
            }
        }

        private void buttonSelect_Click(object sender, EventArgs e)
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

        private void buttonEditProfile_Click(object sender, EventArgs e)
        {
            Process.Start(selectedprofile.FileName);
        }

        private void MapFixer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D && e.Modifiers == Keys.Alt)
            {
            }

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

    class ListBoxProfile
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
    }
}
