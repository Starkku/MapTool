/*
 * Copyright 2017-2020 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 2 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

namespace MapTool.UI
{
    partial class MapToolUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapToolUI));
            this.labelInfo = new System.Windows.Forms.Label();
            this.listFiles = new System.Windows.Forms.ListBox();
            this.contextMenuStripListFiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemListFilesRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemListFilesShowExplorer = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemListFilesSelect = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemListFilesDeselect = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemListFilesRemoveAll = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonConvert = new System.Windows.Forms.Button();
            this.cbOverwrite = new System.Windows.Forms.CheckBox();
            this.listProfiles = new System.Windows.Forms.ListBox();
            this.contextMenuStripListProfiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemListProfilesEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemListProfilesShowExplorer = new System.Windows.Forms.ToolStripMenuItem();
            this.labelListFiles = new System.Windows.Forms.Label();
            this.labelListProfiles = new System.Windows.Forms.Label();
            this.labelProfileDescription = new System.Windows.Forms.Label();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.buttonSelect = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageMain = new System.Windows.Forms.TabPage();
            this.tabPageLogger = new System.Windows.Forms.TabPage();
            this.textBoxLogger = new System.Windows.Forms.RichTextBox();
            this.tabPageAbout = new System.Windows.Forms.TabPage();
            this.linkLabelRefNDesk = new System.Windows.Forms.LinkLabel();
            this.linkLabelRefMiniLZO = new System.Windows.Forms.LinkLabel();
            this.linkLabelRefSHOpen = new System.Windows.Forms.LinkLabel();
            this.linkLabelRefCS = new System.Windows.Forms.LinkLabel();
            this.linkLabelRefTunnel = new System.Windows.Forms.LinkLabel();
            this.labelExtraCredits1 = new System.Windows.Forms.Label();
            this.labelSpecialThanks = new System.Windows.Forms.Label();
            this.linkLabelRefStarkku = new System.Windows.Forms.LinkLabel();
            this.labelAboutOSCode = new System.Windows.Forms.Label();
            this.linkLabelAboutGithub = new System.Windows.Forms.LinkLabel();
            this.labelAboutCopyright = new System.Windows.Forms.Label();
            this.labelExtraCredits2 = new System.Windows.Forms.Label();
            this.contextMenuStripListFiles.SuspendLayout();
            this.contextMenuStripListProfiles.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageMain.SuspendLayout();
            this.tabPageLogger.SuspendLayout();
            this.tabPageAbout.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelInfo
            // 
            this.labelInfo.Location = new System.Drawing.Point(8, 8);
            this.labelInfo.Margin = new System.Windows.Forms.Padding(0);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(408, 56);
            this.labelInfo.TabIndex = 0;
            this.labelInfo.Text = resources.GetString("labelInfo.Text");
            // 
            // listFiles
            // 
            this.listFiles.BackColor = System.Drawing.SystemColors.Control;
            this.listFiles.ContextMenuStrip = this.contextMenuStripListFiles;
            this.listFiles.FormattingEnabled = true;
            this.listFiles.Location = new System.Drawing.Point(8, 239);
            this.listFiles.Margin = new System.Windows.Forms.Padding(0);
            this.listFiles.Name = "listFiles";
            this.listFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listFiles.Size = new System.Drawing.Size(268, 95);
            this.listFiles.TabIndex = 1;
            this.listFiles.SelectedValueChanged += new System.EventHandler(this.ListFiles_SelectedValueChanged);
            this.listFiles.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ListFiles_MouseMove);
            // 
            // contextMenuStripListFiles
            // 
            this.contextMenuStripListFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemListFilesRemove,
            this.toolStripMenuItemListFilesShowExplorer,
            this.toolStripSeparator1,
            this.toolStripMenuItemListFilesSelect,
            this.toolStripMenuItemListFilesDeselect,
            this.toolStripMenuItemListFilesRemoveAll});
            this.contextMenuStripListFiles.Name = "contextMenuStripListFiles";
            this.contextMenuStripListFiles.Size = new System.Drawing.Size(203, 120);
            // 
            // toolStripMenuItemListFilesRemove
            // 
            this.toolStripMenuItemListFilesRemove.Enabled = false;
            this.toolStripMenuItemListFilesRemove.Name = "toolStripMenuItemListFilesRemove";
            this.toolStripMenuItemListFilesRemove.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.toolStripMenuItemListFilesRemove.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemListFilesRemove.Text = "Remove";
            this.toolStripMenuItemListFilesRemove.Click += new System.EventHandler(this.ToolStripItemListFilesRemove_Click);
            // 
            // toolStripMenuItemListFilesShowExplorer
            // 
            this.toolStripMenuItemListFilesShowExplorer.Enabled = false;
            this.toolStripMenuItemListFilesShowExplorer.Name = "toolStripMenuItemListFilesShowExplorer";
            this.toolStripMenuItemListFilesShowExplorer.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.toolStripMenuItemListFilesShowExplorer.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemListFilesShowExplorer.Text = "Show In Explorer";
            this.toolStripMenuItemListFilesShowExplorer.Click += new System.EventHandler(this.ToolStripMenuItemListFilesShowExplorer_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(199, 6);
            // 
            // toolStripMenuItemListFilesSelect
            // 
            this.toolStripMenuItemListFilesSelect.Enabled = false;
            this.toolStripMenuItemListFilesSelect.Name = "toolStripMenuItemListFilesSelect";
            this.toolStripMenuItemListFilesSelect.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.toolStripMenuItemListFilesSelect.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemListFilesSelect.Text = "Select All";
            this.toolStripMenuItemListFilesSelect.Click += new System.EventHandler(this.ToolStripMenuItemListFilesSelect_Click);
            // 
            // toolStripMenuItemListFilesDeselect
            // 
            this.toolStripMenuItemListFilesDeselect.Enabled = false;
            this.toolStripMenuItemListFilesDeselect.Name = "toolStripMenuItemListFilesDeselect";
            this.toolStripMenuItemListFilesDeselect.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.toolStripMenuItemListFilesDeselect.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemListFilesDeselect.Text = "Deselect All";
            this.toolStripMenuItemListFilesDeselect.Click += new System.EventHandler(this.ToolStripMenuItemListFilesDeselect_Click);
            // 
            // toolStripMenuItemListFilesRemoveAll
            // 
            this.toolStripMenuItemListFilesRemoveAll.Enabled = false;
            this.toolStripMenuItemListFilesRemoveAll.Name = "toolStripMenuItemListFilesRemoveAll";
            this.toolStripMenuItemListFilesRemoveAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Delete)));
            this.toolStripMenuItemListFilesRemoveAll.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemListFilesRemoveAll.Text = "Remove All";
            this.toolStripMenuItemListFilesRemoveAll.Click += new System.EventHandler(this.ToolStripItemListFilesRemoveAll_Click);
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(291, 239);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(115, 23);
            this.buttonBrowse.TabIndex = 2;
            this.buttonBrowse.Text = "Browse...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.ButtonBrowse_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Enabled = false;
            this.buttonRemove.Location = new System.Drawing.Point(291, 297);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(115, 23);
            this.buttonRemove.TabIndex = 3;
            this.buttonRemove.Text = "Remove Selected";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Visible = false;
            this.buttonRemove.Click += new System.EventHandler(this.ButtonRemove_Click);
            // 
            // buttonConvert
            // 
            this.buttonConvert.Enabled = false;
            this.buttonConvert.Location = new System.Drawing.Point(291, 336);
            this.buttonConvert.Name = "buttonConvert";
            this.buttonConvert.Size = new System.Drawing.Size(115, 23);
            this.buttonConvert.TabIndex = 5;
            this.buttonConvert.Text = "Process Maps";
            this.buttonConvert.UseVisualStyleBackColor = true;
            this.buttonConvert.Click += new System.EventHandler(this.ButtonConvert_Click);
            // 
            // cbOverwrite
            // 
            this.cbOverwrite.AutoSize = true;
            this.cbOverwrite.Location = new System.Drawing.Point(8, 367);
            this.cbOverwrite.Name = "cbOverwrite";
            this.cbOverwrite.Size = new System.Drawing.Size(356, 17);
            this.cbOverwrite.TabIndex = 6;
            this.cbOverwrite.Text = "Overwrite Original Files (by default it saves a copy with \'_altered\' suffix)";
            this.cbOverwrite.UseVisualStyleBackColor = true;
            // 
            // listProfiles
            // 
            this.listProfiles.BackColor = System.Drawing.SystemColors.Control;
            this.listProfiles.ContextMenuStrip = this.contextMenuStripListProfiles;
            this.listProfiles.FormattingEnabled = true;
            this.listProfiles.Location = new System.Drawing.Point(8, 92);
            this.listProfiles.Name = "listProfiles";
            this.listProfiles.Size = new System.Drawing.Size(268, 95);
            this.listProfiles.TabIndex = 7;
            this.listProfiles.SelectedIndexChanged += new System.EventHandler(this.ListProfiles_SelectedIndexChanged);
            // 
            // contextMenuStripListProfiles
            // 
            this.contextMenuStripListProfiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemListProfilesEdit,
            this.toolStripMenuItemListProfilesShowExplorer});
            this.contextMenuStripListProfiles.Name = "contextMenuStripListProfiles";
            this.contextMenuStripListProfiles.Size = new System.Drawing.Size(205, 48);
            // 
            // toolStripMenuItemListProfilesEdit
            // 
            this.toolStripMenuItemListProfilesEdit.Name = "toolStripMenuItemListProfilesEdit";
            this.toolStripMenuItemListProfilesEdit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.toolStripMenuItemListProfilesEdit.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemListProfilesEdit.Text = "Open Profile File";
            this.toolStripMenuItemListProfilesEdit.Click += new System.EventHandler(this.ToolStripItemListProfilesEdit_Click);
            // 
            // toolStripMenuItemListProfilesShowExplorer
            // 
            this.toolStripMenuItemListProfilesShowExplorer.Name = "toolStripMenuItemListProfilesShowExplorer";
            this.toolStripMenuItemListProfilesShowExplorer.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.toolStripMenuItemListProfilesShowExplorer.Size = new System.Drawing.Size(204, 22);
            this.toolStripMenuItemListProfilesShowExplorer.Text = "Show in Explorer";
            this.toolStripMenuItemListProfilesShowExplorer.Click += new System.EventHandler(this.ToolStripItemListProfilesShowExplorer_Click);
            // 
            // labelListFiles
            // 
            this.labelListFiles.AutoSize = true;
            this.labelListFiles.Location = new System.Drawing.Point(5, 223);
            this.labelListFiles.Name = "labelListFiles";
            this.labelListFiles.Size = new System.Drawing.Size(276, 13);
            this.labelListFiles.TabIndex = 8;
            this.labelListFiles.Text = "Map Files (Browse or drag and drop to add files to the list)";
            // 
            // labelListProfiles
            // 
            this.labelListProfiles.AutoSize = true;
            this.labelListProfiles.Location = new System.Drawing.Point(5, 76);
            this.labelListProfiles.Name = "labelListProfiles";
            this.labelListProfiles.Size = new System.Drawing.Size(143, 13);
            this.labelListProfiles.TabIndex = 9;
            this.labelListProfiles.Text = "Available Conversion Profiles";
            // 
            // labelProfileDescription
            // 
            this.labelProfileDescription.Location = new System.Drawing.Point(291, 92);
            this.labelProfileDescription.Name = "labelProfileDescription";
            this.labelProfileDescription.Size = new System.Drawing.Size(115, 94);
            this.labelProfileDescription.TabIndex = 10;
            // 
            // openFileDialog
            // 
            this.openFileDialog.Multiselect = true;
            // 
            // buttonSelect
            // 
            this.buttonSelect.Enabled = false;
            this.buttonSelect.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSelect.Location = new System.Drawing.Point(291, 268);
            this.buttonSelect.Name = "buttonSelect";
            this.buttonSelect.Size = new System.Drawing.Size(115, 23);
            this.buttonSelect.TabIndex = 11;
            this.buttonSelect.Text = "Select/Deselect All";
            this.buttonSelect.UseVisualStyleBackColor = true;
            this.buttonSelect.Visible = false;
            this.buttonSelect.Click += new System.EventHandler(this.ButtonSelect_Click);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageMain);
            this.tabControl.Controls.Add(this.tabPageLogger);
            this.tabControl.Controls.Add(this.tabPageAbout);
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(427, 418);
            this.tabControl.TabIndex = 13;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl_SelectedIndexChanged);
            this.tabControl.Deselecting += new System.Windows.Forms.TabControlCancelEventHandler(this.TabControl_Deselecting);
            // 
            // tabPageMain
            // 
            this.tabPageMain.Controls.Add(this.labelProfileDescription);
            this.tabPageMain.Controls.Add(this.labelListProfiles);
            this.tabPageMain.Controls.Add(this.labelListFiles);
            this.tabPageMain.Controls.Add(this.listProfiles);
            this.tabPageMain.Controls.Add(this.listFiles);
            this.tabPageMain.Controls.Add(this.labelInfo);
            this.tabPageMain.Controls.Add(this.cbOverwrite);
            this.tabPageMain.Controls.Add(this.buttonConvert);
            this.tabPageMain.Controls.Add(this.buttonRemove);
            this.tabPageMain.Controls.Add(this.buttonBrowse);
            this.tabPageMain.Controls.Add(this.buttonSelect);
            this.tabPageMain.Location = new System.Drawing.Point(4, 22);
            this.tabPageMain.Name = "tabPageMain";
            this.tabPageMain.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMain.Size = new System.Drawing.Size(419, 392);
            this.tabPageMain.TabIndex = 0;
            this.tabPageMain.Text = "Main";
            this.tabPageMain.UseVisualStyleBackColor = true;
            // 
            // tabPageLogger
            // 
            this.tabPageLogger.Controls.Add(this.textBoxLogger);
            this.tabPageLogger.Location = new System.Drawing.Point(4, 22);
            this.tabPageLogger.Name = "tabPageLogger";
            this.tabPageLogger.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLogger.Size = new System.Drawing.Size(419, 392);
            this.tabPageLogger.TabIndex = 1;
            this.tabPageLogger.Text = "Log";
            this.tabPageLogger.UseVisualStyleBackColor = true;
            // 
            // textBoxLogger
            // 
            this.textBoxLogger.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxLogger.Location = new System.Drawing.Point(8, 10);
            this.textBoxLogger.Name = "textBoxLogger";
            this.textBoxLogger.ReadOnly = true;
            this.textBoxLogger.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.textBoxLogger.Size = new System.Drawing.Size(397, 371);
            this.textBoxLogger.TabIndex = 0;
            this.textBoxLogger.Text = "";
            this.textBoxLogger.TextChanged += new System.EventHandler(this.TextBoxLogger_TextChanged);
            // 
            // tabPageAbout
            // 
            this.tabPageAbout.Controls.Add(this.labelExtraCredits2);
            this.tabPageAbout.Controls.Add(this.linkLabelRefNDesk);
            this.tabPageAbout.Controls.Add(this.linkLabelRefMiniLZO);
            this.tabPageAbout.Controls.Add(this.linkLabelRefSHOpen);
            this.tabPageAbout.Controls.Add(this.linkLabelRefCS);
            this.tabPageAbout.Controls.Add(this.linkLabelRefTunnel);
            this.tabPageAbout.Controls.Add(this.labelExtraCredits1);
            this.tabPageAbout.Controls.Add(this.labelSpecialThanks);
            this.tabPageAbout.Controls.Add(this.linkLabelRefStarkku);
            this.tabPageAbout.Controls.Add(this.labelAboutOSCode);
            this.tabPageAbout.Controls.Add(this.linkLabelAboutGithub);
            this.tabPageAbout.Controls.Add(this.labelAboutCopyright);
            this.tabPageAbout.Location = new System.Drawing.Point(4, 22);
            this.tabPageAbout.Name = "tabPageAbout";
            this.tabPageAbout.Size = new System.Drawing.Size(419, 392);
            this.tabPageAbout.TabIndex = 2;
            this.tabPageAbout.Text = "About";
            this.tabPageAbout.UseVisualStyleBackColor = true;
            // 
            // linkLabelRefNDesk
            // 
            this.linkLabelRefNDesk.AutoSize = true;
            this.linkLabelRefNDesk.LinkArea = new System.Windows.Forms.LinkArea(17, 28);
            this.linkLabelRefNDesk.Location = new System.Drawing.Point(8, 154);
            this.linkLabelRefNDesk.Name = "linkLabelRefNDesk";
            this.linkLabelRefNDesk.Size = new System.Drawing.Size(242, 17);
            this.linkLabelRefNDesk.TabIndex = 6;
            this.linkLabelRefNDesk.TabStop = true;
            this.linkLabelRefNDesk.Text = "• NDesk.Options: http://www.ndesk.org/Options";
            this.linkLabelRefNDesk.UseCompatibleTextRendering = true;
            // 
            // linkLabelRefMiniLZO
            // 
            this.linkLabelRefMiniLZO.AutoSize = true;
            this.linkLabelRefMiniLZO.LinkArea = new System.Windows.Forms.LinkArea(19, 35);
            this.linkLabelRefMiniLZO.Location = new System.Drawing.Point(8, 134);
            this.linkLabelRefMiniLZO.Name = "linkLabelRefMiniLZO";
            this.linkLabelRefMiniLZO.Size = new System.Drawing.Size(281, 17);
            this.linkLabelRefMiniLZO.TabIndex = 5;
            this.linkLabelRefMiniLZO.TabStop = true;
            this.linkLabelRefMiniLZO.Text = "• C# MiniLZO port: https://github.com/zzattack/MiniLZO";
            this.linkLabelRefMiniLZO.UseCompatibleTextRendering = true;
            this.linkLabelRefMiniLZO.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_OpenLink);
            // 
            // linkLabelRefSHOpen
            // 
            this.linkLabelRefSHOpen.AutoSize = true;
            this.linkLabelRefSHOpen.LinkArea = new System.Windows.Forms.LinkArea(38, 36);
            this.linkLabelRefSHOpen.Location = new System.Drawing.Point(8, 194);
            this.linkLabelRefSHOpen.Name = "linkLabelRefSHOpen";
            this.linkLabelRefSHOpen.Size = new System.Drawing.Size(398, 17);
            this.linkLabelRefSHOpen.TabIndex = 8;
            this.linkLabelRefSHOpen.TabStop = true;
            this.linkLabelRefSHOpen.Text = "• SHOpenFolderAndSelectItems wrapper: https://gist.github.com/vbfox/551626";
            this.linkLabelRefSHOpen.UseCompatibleTextRendering = true;
            this.linkLabelRefSHOpen.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_OpenLink);
            // 
            // linkLabelRefCS
            // 
            this.linkLabelRefCS.AutoSize = true;
            this.linkLabelRefCS.LinkArea = new System.Windows.Forms.LinkArea(15, 48);
            this.linkLabelRefCS.Location = new System.Drawing.Point(8, 114);
            this.linkLabelRefCS.Name = "linkLabelRefCS";
            this.linkLabelRefCS.Size = new System.Drawing.Size(337, 17);
            this.linkLabelRefCS.TabIndex = 4;
            this.linkLabelRefCS.TabStop = true;
            this.linkLabelRefCS.Text = "• Chronoshift: https://github.com/TheAssemblyArmada/Chronoshift";
            this.linkLabelRefCS.UseCompatibleTextRendering = true;
            this.linkLabelRefCS.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_OpenLink);
            // 
            // linkLabelRefTunnel
            // 
            this.linkLabelRefTunnel.AutoSize = true;
            this.linkLabelRefTunnel.LinkArea = new System.Windows.Forms.LinkArea(30, 43);
            this.linkLabelRefTunnel.Location = new System.Drawing.Point(8, 174);
            this.linkLabelRefTunnel.Name = "linkLabelRefTunnel";
            this.linkLabelRefTunnel.Size = new System.Drawing.Size(390, 17);
            this.linkLabelRefTunnel.TabIndex = 7;
            this.linkLabelRefTunnel.TabStop = true;
            this.linkLabelRefTunnel.Text = "• Rampastring\'s Tunnel Fixer: https://ppmforums.com/viewtopic.php?t=42008";
            this.linkLabelRefTunnel.UseCompatibleTextRendering = true;
            this.linkLabelRefTunnel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_OpenLink);
            // 
            // labelExtraCredits1
            // 
            this.labelExtraCredits1.AutoSize = true;
            this.labelExtraCredits1.Location = new System.Drawing.Point(11, 255);
            this.labelExtraCredits1.Name = "labelExtraCredits1";
            this.labelExtraCredits1.Size = new System.Drawing.Size(396, 26);
            this.labelExtraCredits1.TabIndex = 10;
            this.labelExtraCredits1.Text = "• E1 Elite: Implemented IsoMapPack5 optimization and ice growth fix features and " +
    "\r\ncreated several TS conversion profiles included with releases.";
            // 
            // labelSpecialThanks
            // 
            this.labelSpecialThanks.AutoSize = true;
            this.labelSpecialThanks.Location = new System.Drawing.Point(8, 228);
            this.labelSpecialThanks.Name = "labelSpecialThanks";
            this.labelSpecialThanks.Size = new System.Drawing.Size(182, 13);
            this.labelSpecialThanks.TabIndex = 9;
            this.labelSpecialThanks.Text = "Additional thanks to following people:";
            // 
            // linkLabelRefStarkku
            // 
            this.linkLabelRefStarkku.AutoSize = true;
            this.linkLabelRefStarkku.LinkArea = new System.Windows.Forms.LinkArea(21, 44);
            this.linkLabelRefStarkku.Location = new System.Drawing.Point(8, 94);
            this.linkLabelRefStarkku.Name = "linkLabelRefStarkku";
            this.linkLabelRefStarkku.Size = new System.Drawing.Size(308, 17);
            this.linkLabelRefStarkku.TabIndex = 3;
            this.linkLabelRefStarkku.TabStop = true;
            this.linkLabelRefStarkku.Text = "• Starkku.Utilities: https://github.com/Starkku/Starkku.Utilities";
            this.linkLabelRefStarkku.UseCompatibleTextRendering = true;
            this.linkLabelRefStarkku.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_OpenLink);
            // 
            // labelAboutOSCode
            // 
            this.labelAboutOSCode.AutoSize = true;
            this.labelAboutOSCode.Location = new System.Drawing.Point(8, 74);
            this.labelAboutOSCode.Name = "labelAboutOSCode";
            this.labelAboutOSCode.Size = new System.Drawing.Size(296, 13);
            this.labelAboutOSCode.TabIndex = 2;
            this.labelAboutOSCode.Text = "Code from following open-source projects is used in MapTool:";
            // 
            // linkLabelAboutGithub
            // 
            this.linkLabelAboutGithub.AutoSize = true;
            this.linkLabelAboutGithub.LinkArea = new System.Windows.Forms.LinkArea(19, 34);
            this.linkLabelAboutGithub.Location = new System.Drawing.Point(8, 36);
            this.linkLabelAboutGithub.Name = "linkLabelAboutGithub";
            this.linkLabelAboutGithub.Size = new System.Drawing.Size(287, 17);
            this.linkLabelAboutGithub.TabIndex = 1;
            this.linkLabelAboutGithub.TabStop = true;
            this.linkLabelAboutGithub.Text = "MapTool on GitHub: https://github.com/Starkku/MapTool";
            this.linkLabelAboutGithub.UseCompatibleTextRendering = true;
            this.linkLabelAboutGithub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_OpenLink);
            // 
            // labelAboutCopyright
            // 
            this.labelAboutCopyright.AutoSize = true;
            this.labelAboutCopyright.Location = new System.Drawing.Point(8, 16);
            this.labelAboutCopyright.Name = "labelAboutCopyright";
            this.labelAboutCopyright.Size = new System.Drawing.Size(166, 13);
            this.labelAboutCopyright.TabIndex = 0;
            this.labelAboutCopyright.Text = "Program by Starkku © 2017-2020";
            // 
            // labelExtraCredits2
            // 
            this.labelExtraCredits2.AutoSize = true;
            this.labelExtraCredits2.Location = new System.Drawing.Point(11, 288);
            this.labelExtraCredits2.Name = "labelExtraCredits2";
            this.labelExtraCredits2.Size = new System.Drawing.Size(353, 13);
            this.labelExtraCredits2.TabIndex = 11;
            this.labelExtraCredits2.Text = "• Messiah: Cross-game theater conversion profiles included with releases.";
            // 
            // MapToolUI
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(421, 415);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::MapTool.UI.Properties.Resources.Icon;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "MapToolUI";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MapTool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MapToolUI_FormClosing);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MapToolUI_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MapToolUI_DragEnter);
            this.contextMenuStripListFiles.ResumeLayout(false);
            this.contextMenuStripListProfiles.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPageMain.ResumeLayout(false);
            this.tabPageMain.PerformLayout();
            this.tabPageLogger.ResumeLayout(false);
            this.tabPageAbout.ResumeLayout(false);
            this.tabPageAbout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.ListBox listFiles;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonConvert;
        private System.Windows.Forms.CheckBox cbOverwrite;
        private System.Windows.Forms.ListBox listProfiles;
        private System.Windows.Forms.Label labelListFiles;
        private System.Windows.Forms.Label labelListProfiles;
        private System.Windows.Forms.Label labelProfileDescription;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button buttonSelect;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageMain;
        private System.Windows.Forms.TabPage tabPageLogger;
        private System.Windows.Forms.RichTextBox textBoxLogger;
        private System.Windows.Forms.TabPage tabPageAbout;
        private System.Windows.Forms.LinkLabel linkLabelAboutGithub;
        private System.Windows.Forms.Label labelAboutCopyright;
        private System.Windows.Forms.Label labelAboutOSCode;
        private System.Windows.Forms.LinkLabel linkLabelRefStarkku;
        private System.Windows.Forms.Label labelSpecialThanks;
        private System.Windows.Forms.Label labelExtraCredits1;
        private System.Windows.Forms.LinkLabel linkLabelRefTunnel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripListFiles;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemListFilesRemove;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemListFilesRemoveAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemListFilesSelect;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemListFilesDeselect;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemListFilesShowExplorer;
        private System.Windows.Forms.LinkLabel linkLabelRefCS;
        private System.Windows.Forms.LinkLabel linkLabelRefSHOpen;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripListProfiles;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemListProfilesEdit;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemListProfilesShowExplorer;
        private System.Windows.Forms.LinkLabel linkLabelRefMiniLZO;
        private System.Windows.Forms.LinkLabel linkLabelRefNDesk;
        private System.Windows.Forms.Label labelExtraCredits2;
    }
}

