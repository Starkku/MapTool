/*
 * Copyright 2017 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
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
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonConvert = new System.Windows.Forms.Button();
            this.cbOverwrite = new System.Windows.Forms.CheckBox();
            this.listProfiles = new System.Windows.Forms.ListBox();
            this.labelListFiles = new System.Windows.Forms.Label();
            this.labelListProfiles = new System.Windows.Forms.Label();
            this.labelProfileDescription = new System.Windows.Forms.Label();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.buttonSelect = new System.Windows.Forms.Button();
            this.buttonEditProfile = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageMain = new System.Windows.Forms.TabPage();
            this.tabPageLogger = new System.Windows.Forms.TabPage();
            this.textBoxLogger = new System.Windows.Forms.RichTextBox();
            this.tabPageAbout = new System.Windows.Forms.TabPage();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.labelSpecialThanks = new System.Windows.Forms.Label();
            this.linkLabelRenderer = new System.Windows.Forms.LinkLabel();
            this.linkLabelAboutOpenRA = new System.Windows.Forms.LinkLabel();
            this.labelAboutOSCode = new System.Windows.Forms.Label();
            this.linkLabelAboutGithub = new System.Windows.Forms.LinkLabel();
            this.labelAboutCopyright = new System.Windows.Forms.Label();
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
            this.labelInfo.Size = new System.Drawing.Size(402, 56);
            this.labelInfo.TabIndex = 0;
            this.labelInfo.Text = resources.GetString("labelInfo.Text");
            // 
            // listFiles
            // 
            this.listFiles.AllowDrop = true;
            this.listFiles.BackColor = System.Drawing.SystemColors.Control;
            this.listFiles.FormattingEnabled = true;
            this.listFiles.Location = new System.Drawing.Point(8, 239);
            this.listFiles.Margin = new System.Windows.Forms.Padding(0);
            this.listFiles.Name = "listFiles";
            this.listFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listFiles.Size = new System.Drawing.Size(268, 95);
            this.listFiles.TabIndex = 1;
            this.listFiles.SelectedValueChanged += new System.EventHandler(this.ListFiles_SelectedValueChanged);
            this.listFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.ListFiles_DragDrop);
            this.listFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.ListFiles_DragEnter);
            this.listFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListFiles_KeyDown);
            this.listFiles.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ListFiles_MouseMove);
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
            this.listProfiles.FormattingEnabled = true;
            this.listProfiles.Location = new System.Drawing.Point(8, 92);
            this.listProfiles.Name = "listProfiles";
            this.listProfiles.Size = new System.Drawing.Size(268, 95);
            this.listProfiles.TabIndex = 7;
            this.listProfiles.SelectedIndexChanged += new System.EventHandler(this.ListProfiles_SelectedIndexChanged);
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
            this.buttonSelect.Click += new System.EventHandler(this.ButtonSelect_Click);
            // 
            // buttonEditProfile
            // 
            this.buttonEditProfile.Enabled = false;
            this.buttonEditProfile.Location = new System.Drawing.Point(383, 92);
            this.buttonEditProfile.Name = "buttonEditProfile";
            this.buttonEditProfile.Size = new System.Drawing.Size(115, 23);
            this.buttonEditProfile.TabIndex = 12;
            this.buttonEditProfile.Text = "Edit Profile";
            this.buttonEditProfile.UseVisualStyleBackColor = true;
            this.buttonEditProfile.Visible = false;
            this.buttonEditProfile.Click += new System.EventHandler(this.ButtonEditProfile_Click);
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
            this.tabPageMain.Controls.Add(this.buttonEditProfile);
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
            // 
            // tabPageAbout
            // 
            this.tabPageAbout.Controls.Add(this.linkLabel1);
            this.tabPageAbout.Controls.Add(this.label1);
            this.tabPageAbout.Controls.Add(this.labelSpecialThanks);
            this.tabPageAbout.Controls.Add(this.linkLabelRenderer);
            this.tabPageAbout.Controls.Add(this.linkLabelAboutOpenRA);
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
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.LinkArea = new System.Windows.Forms.LinkArea(30, 43);
            this.linkLabel1.Location = new System.Drawing.Point(8, 134);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(394, 17);
            this.linkLabel1.TabIndex = 7;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Rampastring\'s Tunnel Fixer - (https://ppmforums.com/viewtopic.php?t=42008)";
            this.linkLabel1.UseCompatibleTextRendering = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 195);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(387, 26);
            this.label1.TabIndex = 6;
            this.label1.Text = "E1 Elite (Implemented IsoMapPack5 optimization and ice growth fix features and \r\n" +
    "created several TS conversion profiles included with releases)";
            // 
            // labelSpecialThanks
            // 
            this.labelSpecialThanks.AutoSize = true;
            this.labelSpecialThanks.Location = new System.Drawing.Point(8, 168);
            this.labelSpecialThanks.Name = "labelSpecialThanks";
            this.labelSpecialThanks.Size = new System.Drawing.Size(182, 13);
            this.labelSpecialThanks.TabIndex = 5;
            this.labelSpecialThanks.Text = "Additional thanks to following people:";
            // 
            // linkLabelRenderer
            // 
            this.linkLabelRenderer.AutoSize = true;
            this.linkLabelRenderer.LinkArea = new System.Windows.Forms.LinkArea(18, 38);
            this.linkLabelRenderer.Location = new System.Drawing.Point(8, 94);
            this.linkLabelRenderer.Name = "linkLabelRenderer";
            this.linkLabelRenderer.Size = new System.Drawing.Size(311, 17);
            this.linkLabelRenderer.TabIndex = 4;
            this.linkLabelRenderer.TabStop = true;
            this.linkLabelRenderer.Text = "CnCMaps Renderer (https://github.com/zzattack/ccmaps-net)";
            this.linkLabelRenderer.UseCompatibleTextRendering = true;
            this.linkLabelRenderer.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelRenderer_LinkClicked);
            // 
            // linkLabelAboutOpenRA
            // 
            this.linkLabelAboutOpenRA.AutoSize = true;
            this.linkLabelAboutOpenRA.LinkArea = new System.Windows.Forms.LinkArea(10, 32);
            this.linkLabelAboutOpenRA.Location = new System.Drawing.Point(8, 114);
            this.linkLabelAboutOpenRA.Name = "linkLabelAboutOpenRA";
            this.linkLabelAboutOpenRA.Size = new System.Drawing.Size(248, 17);
            this.linkLabelAboutOpenRA.TabIndex = 3;
            this.linkLabelAboutOpenRA.TabStop = true;
            this.linkLabelAboutOpenRA.Text = "OpenRA - (https://github.com/OpenRA/OpenRA)";
            this.linkLabelAboutOpenRA.UseCompatibleTextRendering = true;
            this.linkLabelAboutOpenRA.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelAboutOpenRA_LinkClicked);
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
            this.linkLabelAboutGithub.Location = new System.Drawing.Point(8, 36);
            this.linkLabelAboutGithub.Name = "linkLabelAboutGithub";
            this.linkLabelAboutGithub.Size = new System.Drawing.Size(100, 13);
            this.linkLabelAboutGithub.TabIndex = 1;
            this.linkLabelAboutGithub.TabStop = true;
            this.linkLabelAboutGithub.Text = "MapTool on GitHub";
            this.linkLabelAboutGithub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelAboutGithub_LinkClicked);
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
            // MapTool_UI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(421, 415);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::MapTool.UI.Properties.Resources.Icon;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "MapTool_UI";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MapTool";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MapFixer_KeyDown);
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
        private System.Windows.Forms.Button buttonEditProfile;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageMain;
        private System.Windows.Forms.TabPage tabPageLogger;
        private System.Windows.Forms.RichTextBox textBoxLogger;
        private System.Windows.Forms.TabPage tabPageAbout;
        private System.Windows.Forms.LinkLabel linkLabelAboutGithub;
        private System.Windows.Forms.Label labelAboutCopyright;
        private System.Windows.Forms.LinkLabel linkLabelAboutOpenRA;
        private System.Windows.Forms.Label labelAboutOSCode;
        private System.Windows.Forms.LinkLabel linkLabelRenderer;
        private System.Windows.Forms.Label labelSpecialThanks;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}

