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
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using MapTool.Logic;
using Starkku.Utilities;
using Starkku.Utilities.FileTypes;
using System.Threading.Tasks;

namespace MapTool.UI
{
    public partial class MapToolUI : Form
    {
        private readonly List<string> validMapFileExtensions = new List<string> { ".map", ".mpr", ".yrm" };
        private readonly string profileDirectory = AppDomain.CurrentDomain.BaseDirectory + "Profiles";
        private readonly List<ListBoxProfile> conversionProfiles = new List<ListBoxProfile>();
        private ListBoxProfile selectedConversionProfile = null;
        private CancellationTokenSource processMapsTaskTokenSource;
        private int currentHoverIndex = -1;
        private bool processMapsInProgress = false;
        private bool processMapsCanceled = false;
        private bool allowChangingTabs = true;
        private bool closingForm = true;
        private bool writeLogFile = false;
#if DEBUG
        private bool showDebugLog = false;
#else
        private bool showDebugLog = false;
#endif

        public MapToolUI(string[] args)
        {
            ParseArguments(args);
            LoadProfiles();
            InitializeComponent();

            listProfiles.DataSource = conversionProfiles;

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            Text += " v." + version.ToString();

            SetOpenFileDialogFilters();

            string logfile = AppDomain.CurrentDomain.BaseDirectory + Path.ChangeExtension(AppDomain.CurrentDomain.FriendlyName, ".log");
            Logger.Initialize(AddLogMessageToTextBox, logfile, writeLogFile, showDebugLog);
            Logger.WriteTimestamps = false;
        }

        private void ParseArguments(string[] args)
        {
            foreach (string arg in args)
            {
                switch (arg.ToLower())
                {
                    case "-log":
                    case "--log":
                    case "-g":
                        writeLogFile = true;
                        continue;
                    case "-debug":
                    case "--debug":
                    case "-d":
                        showDebugLog = true;
                        continue;
                    default:
                        continue;
                }
            }
        }

        private void SetOpenFileDialogFilters()
        {
            string extensions1 = "", extensions2 = "";
            for (int i = 0; i < validMapFileExtensions.Count; i++)
            {
                string delim;
                string delim2;

                if (i == 0)
                {
                    delim = "";
                    delim2 = "";
                }
                else
                {
                    delim = ",";
                    delim2 = ";";
                }

                extensions1 += delim + "*" + validMapFileExtensions[i];
                extensions2 += delim2 + "*" + validMapFileExtensions[i];
            }

            openFileDialog.Filter = "Map files (" + extensions1 + ")|" + extensions2;
        }

        private void LoadProfiles()
        {
            string[] files = new string[0];

            if (!Directory.Exists(profileDirectory))
            {
                MessageBox.Show("Could not find the profile directory (sub-directory called 'Profiles' in the program directory). Aborting.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                Close();
                Environment.Exit(1);
            }
            else
            {
                files = Directory.GetFiles(profileDirectory, "*.ini", SearchOption.TopDirectoryOnly);

                if (files.Length < 1)
                {
                    MessageBox.Show("Could not find any conversion profiles in the profile directory (sub-directory called 'Profiles' in the program directory). Aborting.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    Close();
                    Environment.Exit(1);
                }
            }

            foreach (string s in files)
            {
                try
                {
                    INIFile profile = new INIFile(s);

                    if (!profile.SectionExists("ProfileData"))
                        continue;

                    conversionProfiles.Add(new ListBoxProfile(s, profile.GetKey("ProfileData", "Name", Path.GetFileName(s)), profile.GetKey("ProfileData", "Description", "Description Not Available")));
                }
                catch (Exception)
                {
                    continue;
                }
            }

            conversionProfiles.Sort();
        }

        private bool CheckIfDuplicate(string filename)
        {
            foreach (ListBoxFile f in listFiles.Items)
            {
                if (f.Filename.Equals(filename))
                    return true;
            }

            return false;
        }

        private void UpdateMapListOptions()
        {
            buttonConvert.Enabled = listFiles.Items.Count > 0 && selectedConversionProfile != null;

            if (listFiles.Items.Count < 1)
            {
                buttonSelect.Enabled = false;
                toolStripMenuItemListFilesSelect.Enabled = false;
                toolStripMenuItemListFilesDeselect.Enabled = false;
                toolStripMenuItemListFilesRemoveAll.Enabled = false;
            }
            else
            {
                buttonSelect.Enabled = true;
                toolStripMenuItemListFilesRemoveAll.Enabled = true;
                toolStripMenuItemListFilesSelect.Enabled = true;
            }

            if (listFiles.SelectedIndices.Count > 0)
            {
                buttonRemove.Enabled = true;
                toolStripMenuItemListFilesRemove.Enabled = true;
                toolStripMenuItemListFilesDeselect.Enabled = true;
                toolStripMenuItemListFilesShowExplorer.Enabled = true;

                if (listFiles.SelectedIndices.Count == listFiles.Items.Count)
                    toolStripMenuItemListFilesSelect.Enabled = false;
                else if (listFiles.Items.Count > 0)
                    toolStripMenuItemListFilesSelect.Enabled = true;
            }
            else
            {
                buttonRemove.Enabled = false;
                toolStripMenuItemListFilesRemove.Enabled = false;
                toolStripMenuItemListFilesDeselect.Enabled = false;
                toolStripMenuItemListFilesShowExplorer.Enabled = false;

                if (listFiles.Items.Count > 0)
                    toolStripMenuItemListFilesSelect.Enabled = true;
            }

        }

        private void ShowSelectedMapInExplorer()
        {
            List<string> filenames = new List<string>(listFiles.SelectedIndices.Count);

            for (int i = 0; i < listFiles.SelectedIndices.Count; i++)
            {
                string filename = (listFiles.Items[i] as ListBoxFile).Filename;

                if (File.Exists(filename))
                    filenames.Add(filename);
            }

            ShowSelectedInExplorer.FilesOrFolders(filenames);
        }

        private void ShowSelectedProfileInExplorer()
        {
            if (listProfiles.SelectedIndex < 0)
                return;

            ShowSelectedInExplorer.FileOrFolder(selectedConversionProfile.Filename);
        }

        private void RemoveSelectedMaps()
        {
            for (int i = listFiles.SelectedIndices.Count - 1; i >= 0; i--)
            {
                listFiles.Items.RemoveAt(listFiles.SelectedIndices[i]);
            }

            UpdateMapListOptions();
        }

        private void RemoveAllMaps()
        {
            listFiles.Items.Clear();
            UpdateMapListOptions();
        }

        private int AddMapFiles(string[] filenames)
        {
            int count = 0;

            foreach (string filename in filenames)
            {
                string ext = Path.GetExtension(filename);

                if (!validMapFileExtensions.Contains(ext))
                    continue;

                if (CheckIfDuplicate(filename))
                    continue;

                listFiles.Items.Add(new ListBoxFile(filename, Path.GetFileName(filename), filename));
                count++;
            }

            UpdateMapListOptions();

            int visibleItems = listFiles.ClientSize.Height / listFiles.ItemHeight;
            listFiles.TopIndex = Math.Max(listFiles.Items.Count - visibleItems + 1, 0);

            return count;
        }

        private void DeselectAllMaps() => listFiles.SelectedIndex = -1;

        private void SelectAllMaps()
        {
            for (int i = 0; i < listFiles.Items.Count; i++)
            {
                listFiles.SetSelected(i, true);
            }
        }

        private void ButtonConvert_Click(object sender, EventArgs e)
        {
            tabControl.SelectedTab = tabPageLogger;
            processMapsCanceled = false;
            textBoxLogger.Text = "";
            ToggleControlState(false);
            processMapsInProgress = true;
            Logger.Info("Starting processing of maps.");
            WriteLogMessage("");
            processMapsTaskTokenSource = new CancellationTokenSource();
            Task task = Task.Factory.StartNew(() => ProcessMaps(processMapsTaskTokenSource.Token), processMapsTaskTokenSource.Token);
            task.ContinueWith(ProcessMapsFinished);
        }

        private void ProcessMaps(CancellationToken cancellationToken)
        {
            try
            {
                foreach (ListBoxFile f in listFiles.Items)
                {
                    ProcessMap(f.Filename);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException)
            {
                processMapsCanceled = true;
            }
        }

        private void ProcessMapsFinished(Task task)
        {
            ToggleControlState(true);
            processMapsInProgress = false;

            if (task.IsCompleted && processMapsCanceled)
                Logger.Info("Processing of maps canceled.");
            else if (task.IsFaulted)
                Logger.Info("Processing of maps canceled due to an error.");
            else
                Logger.Info("Processing of all maps completed successfully.");

            if (closingForm)
            {
                Invoke((MethodInvoker)delegate
                {
                    Close();
                });
            }
        }

        private void ProcessMap(string filename)
        {
            Logger.Info("Processing map: " + filename);

            string outputfilename = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(filename) + "_altered" + Path.GetExtension(filename);

            if (cbOverwrite.Checked)
                outputfilename = filename;

            MapFileTool mapTool = new MapFileTool(filename, outputfilename, selectedConversionProfile.Filename, false);

            if (mapTool.Initialized)
            {
                mapTool.ConvertTileData();
                mapTool.ConvertOverlayData();
                mapTool.ConvertObjectData();
                mapTool.ConvertSectionData();
                mapTool.ConvertTheaterData();
                mapTool.Save();
            }

            WriteLogMessage("");
        }

        private void WriteLogMessage(string logMessage)
        {
            AddLogMessageToTextBox(logMessage);
            Logger.LogToFileOnly(logMessage);
        }

        private delegate void LogDelegate(string logMessage);

        private void AddLogMessageToTextBox(string logMessage)
        {
            if (logMessage == null)
                return;

            if (InvokeRequired)
            {
                Invoke(new LogDelegate(AddLogMessageToTextBox), logMessage);
                return;
            }

            textBoxLogger.AppendText(logMessage + Environment.NewLine);
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
            allowChangingTabs = enable;
        }

        private void MapToolUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (processMapsInProgress)
            {
                DialogResult dialogResult = MessageBox.Show("MapTool is still processing maps. If you are certain you want to close the program, choose 'Yes' and it will close after current map has been processed.",
                    "Map Processing In Progress", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (dialogResult == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (processMapsTaskTokenSource != null && !processMapsTaskTokenSource.IsCancellationRequested)
            {
                e.Cancel = true;
                processMapsTaskTokenSource.Cancel();
                closingForm = true;
            }
        }

        private void MapToolUI_DragEnter(object sender, DragEventArgs e)
        {
            if (processMapsInProgress)
                return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void MapToolUI_DragDrop(object sender, DragEventArgs e)
        {
            if (processMapsInProgress)
                return;

            List<string> filenames = new List<string>();

            foreach (string s in (string[])e.Data.GetData(DataFormats.FileDrop, false))
            {
                if (Directory.Exists(s))
                    filenames.AddRange(Directory.GetFiles(s));
                else
                    filenames.Add(s);
            }

            int count = AddMapFiles(filenames.ToArray());

            if (count > 0)
                tabControl.SelectedTab = tabPageMain;
        }

        private void TabControl_Deselecting(object sender, TabControlCancelEventArgs e)
        {
            if (!allowChangingTabs)
                e.Cancel = true;
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabPageLogger && processMapsInProgress)
                textBoxLogger.Focus();
        }

        private void ListProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                selectedConversionProfile = conversionProfiles[listProfiles.SelectedIndex];
                labelProfileDescription.Text = selectedConversionProfile.Description;
            }
            catch (Exception)
            {
                selectedConversionProfile = null;
            }

            if (selectedConversionProfile == null || listFiles.Items.Count < 1)
                buttonConvert.Enabled = false;
        }

        private void ListFiles_MouseMove(object sender, MouseEventArgs e)
        {
            int newHoverIndex = listFiles.IndexFromPoint(e.Location);

            if (currentHoverIndex != newHoverIndex)
            {
                currentHoverIndex = newHoverIndex;

                if (currentHoverIndex > -1)
                {
                    toolTip.Active = false;
                    ListBoxFile f = listFiles.Items[currentHoverIndex] as ListBoxFile;
                    toolTip.SetToolTip(listFiles, f.Tooltip);
                    toolTip.Active = true;
                }
            }
        }

        private void ListFiles_SelectedValueChanged(object sender, EventArgs e) => UpdateMapListOptions();

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                AddMapFiles(openFileDialog.FileNames);
        }

        private void ButtonSelect_Click(object sender, EventArgs e)
        {
            if (listFiles.SelectedIndices.Count > 0)
                DeselectAllMaps();
            else
                SelectAllMaps();
        }

        private void ButtonRemove_Click(object sender, EventArgs e) => RemoveSelectedMaps();

        private void TextBoxLogger_TextChanged(object sender, EventArgs e)
        {
            if (!textBoxLogger.Focused && tabControl.SelectedTab == tabPageLogger)
                textBoxLogger.Focus();
        }

        private void LinkLabel_OpenLink(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!(sender is LinkLabel))
                return;

            LinkLabel linkLabel = sender as LinkLabel;
            string url;

            if (e.Link.LinkData != null)
                url = e.Link.LinkData.ToString();
            else
                url = linkLabel.Text.Substring(e.Link.Start, e.Link.Length);

            if (!url.Contains("://"))
                url = "https://" + url;

            Process.Start(url);
            linkLabel.LinkVisited = true;
        }

        private void ToolStripItemListFilesRemove_Click(object sender, EventArgs e) => RemoveSelectedMaps();

        private void ToolStripItemListFilesRemoveAll_Click(object sender, EventArgs e) => RemoveAllMaps();

        private void ToolStripMenuItemListFilesSelect_Click(object sender, EventArgs e) => SelectAllMaps();

        private void ToolStripMenuItemListFilesDeselect_Click(object sender, EventArgs e) => DeselectAllMaps();

        private void ToolStripMenuItemListFilesShowExplorer_Click(object sender, EventArgs e) => ShowSelectedMapInExplorer();

        private void ToolStripItemListProfilesEdit_Click(object sender, EventArgs e)
        {
            if (File.Exists(selectedConversionProfile.Filename))
                Process.Start(selectedConversionProfile.Filename);
        }

        private void ToolStripItemListProfilesShowExplorer_Click(object sender, EventArgs e) => ShowSelectedProfileInExplorer();
    }

    internal class ListBoxFile
    {
        public string Filename { get; set; }
        public string DisplayName { get; set; }
        public string Tooltip { get; set; }

        public ListBoxFile(string filename, string displayName, string tooltip)
        {
            Filename = filename;
            DisplayName = displayName;
            Tooltip = tooltip;
        }

        public override string ToString() => DisplayName;

    }

    internal class ListBoxProfile : IComparable<ListBoxProfile>
    {
        public string Filename { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ListBoxProfile(string filename, string name, string description)
        {
            Filename = filename;
            Name = name;
            Description = description;
        }

        public override string ToString() => Name;

        public int CompareTo(ListBoxProfile other) => Name.CompareTo(other.Name);

    }
}
