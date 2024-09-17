// Copyright (c) 2024 Nubsuki
// All rights reserved.
// This application is free to use, modify, and distribute for personal and educational purposes.
// Commercial use or selling of this code is not permitted without explicit permission.

namespace CyberpunkModManager
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanelModCards = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.selectFolderButton = new System.Windows.Forms.Button();
            this.openGameRootButton = new System.Windows.Forms.Button();
            this.openModMovedButton = new System.Windows.Forms.Button();
            this.importModOrderButton = new System.Windows.Forms.Button();
            this.installationProgressBar = new System.Windows.Forms.ProgressBar();
            this.enableUninstallCheckBox = new System.Windows.Forms.CheckBox(); // Add this line
            this.tableLayoutPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.95652F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 73.04348F));
            this.tableLayoutPanel.Controls.Add(this.flowLayoutPanelModCards, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.buttonPanel, 0, 0);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 1;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(920, 522);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // flowLayoutPanelModCards
            // 
            this.flowLayoutPanelModCards.AutoScroll = true;
            this.flowLayoutPanelModCards.BackColor = System.Drawing.SystemColors.Control;
            this.flowLayoutPanelModCards.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelModCards.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelModCards.Location = new System.Drawing.Point(251, 3);
            this.flowLayoutPanelModCards.Name = "flowLayoutPanelModCards";
            this.flowLayoutPanelModCards.Size = new System.Drawing.Size(666, 516);
            this.flowLayoutPanelModCards.TabIndex = 1;
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.selectFolderButton);
            this.buttonPanel.Controls.Add(this.openGameRootButton);
            this.buttonPanel.Controls.Add(this.openModMovedButton);
            this.buttonPanel.Controls.Add(this.importModOrderButton);
            this.buttonPanel.Controls.Add(this.installationProgressBar);
            this.buttonPanel.Controls.Add(this.enableUninstallCheckBox); // Add to button panel
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.buttonPanel.Location = new System.Drawing.Point(3, 3);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(242, 516);
            this.buttonPanel.TabIndex = 2;
            // 
            // selectFolderButton
            // 
            this.selectFolderButton.Location = new System.Drawing.Point(3, 3);
            this.selectFolderButton.Name = "selectFolderButton";
            this.selectFolderButton.Size = new System.Drawing.Size(237, 30);
            this.selectFolderButton.TabIndex = 0;
            this.selectFolderButton.Text = "Select Game Root Folder";
            this.selectFolderButton.UseVisualStyleBackColor = true;
            this.selectFolderButton.Click += new System.EventHandler(this.SelectFolderButton_Click);
            // 
            // openGameRootButton
            // 
            this.openGameRootButton.Location = new System.Drawing.Point(3, 39);
            this.openGameRootButton.Name = "openGameRootButton";
            this.openGameRootButton.Size = new System.Drawing.Size(237, 30);
            this.openGameRootButton.TabIndex = 1;
            this.openGameRootButton.Text = "Open Game Root Folder";
            this.openGameRootButton.UseVisualStyleBackColor = true;
            this.openGameRootButton.Click += new System.EventHandler(this.OpenGameRootButton_Click);
            // 
            // openModMovedButton
            // 
            this.openModMovedButton.Location = new System.Drawing.Point(3, 75);
            this.openModMovedButton.Name = "openModMovedButton";
            this.openModMovedButton.Size = new System.Drawing.Size(237, 30);
            this.openModMovedButton.TabIndex = 2;
            this.openModMovedButton.Text = "Open Mod Moved Folder";
            this.openModMovedButton.UseVisualStyleBackColor = true;
            this.openModMovedButton.Click += new System.EventHandler(this.OpenModMovedButton_Click);
            // 
            // importModOrderButton
            // 
            this.importModOrderButton.Location = new System.Drawing.Point(3, 111);
            this.importModOrderButton.Name = "importModOrderButton";
            this.importModOrderButton.Size = new System.Drawing.Size(237, 30);
            this.importModOrderButton.TabIndex = 3;
            this.importModOrderButton.Text = "Import Mod Order";
            this.importModOrderButton.UseVisualStyleBackColor = true;
            this.importModOrderButton.Click += new System.EventHandler(this.ImportModOrderButton_Click);
            // 
            // installationProgressBar
            // 
            this.installationProgressBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.installationProgressBar.Location = new System.Drawing.Point(3, 147);
            this.installationProgressBar.Name = "installationProgressBar";
            this.installationProgressBar.Size = new System.Drawing.Size(237, 30);
            this.installationProgressBar.TabIndex = 4;
            // 
            // enableUninstallCheckBox
            // 
            this.enableUninstallCheckBox.AutoSize = true;
            this.enableUninstallCheckBox.Location = new System.Drawing.Point(3, 183); // Adjust position as needed
            this.enableUninstallCheckBox.Name = "enableUninstallCheckBox";
            this.enableUninstallCheckBox.Size = new System.Drawing.Size(237, 17);
            this.enableUninstallCheckBox.TabIndex = 6; // Update tab index
            this.enableUninstallCheckBox.Text = "Enable Uninstall Buttons (Do at your own risk)";
            this.enableUninstallCheckBox.UseVisualStyleBackColor = true;
            this.enableUninstallCheckBox.CheckedChanged += new System.EventHandler(this.EnableUninstallCheckBox_CheckedChanged); // Add event handler
            // 
            // Form1
            // 
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(920, 522);
            this.Controls.Add(this.tableLayoutPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Cyberpunk Mod Manager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanel.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelModCards;
        private System.Windows.Forms.FlowLayoutPanel buttonPanel;
        private System.Windows.Forms.Button selectFolderButton;
        private System.Windows.Forms.Button openGameRootButton;
        private System.Windows.Forms.Button openModMovedButton;
        private System.Windows.Forms.Button importModOrderButton;
        private System.Windows.Forms.ProgressBar installationProgressBar;
        private System.Windows.Forms.CheckBox enableUninstallCheckBox; // Add this line
        #endregion
    }
}

