﻿namespace PuppetMasterUI
{
    partial class ScriptRunner
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptRunner));
            this.tsTopNavigation = new System.Windows.Forms.ToolStrip();
            this.tsOpenScript = new System.Windows.Forms.ToolStripButton();
            this.tsSaveScript = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsRunScript = new System.Windows.Forms.ToolStripButton();
            this.tsRunScriptStep = new System.Windows.Forms.ToolStripButton();
            this.tsStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsCleanScript = new System.Windows.Forms.ToolStripButton();
            this.ofdOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.sfdSaveFile = new System.Windows.Forms.SaveFileDialog();
            this.tcScriptContainer = new System.Windows.Forms.TabControl();
            this.tpNewScript = new System.Windows.Forms.TabPage();
            this.txtScripts = new System.Windows.Forms.TextBox();
            this.tsTopNavigation.SuspendLayout();
            this.tcScriptContainer.SuspendLayout();
            this.tpNewScript.SuspendLayout();
            this.SuspendLayout();
            // 
            // tsTopNavigation
            // 
            this.tsTopNavigation.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsOpenScript,
            this.tsSaveScript,
            this.toolStripSeparator1,
            this.tsRunScript,
            this.tsRunScriptStep,
            this.tsStop,
            this.toolStripSeparator2,
            this.tsCleanScript});
            this.tsTopNavigation.Location = new System.Drawing.Point(0, 0);
            this.tsTopNavigation.Name = "tsTopNavigation";
            this.tsTopNavigation.Padding = new System.Windows.Forms.Padding(0);
            this.tsTopNavigation.Size = new System.Drawing.Size(887, 25);
            this.tsTopNavigation.TabIndex = 1;
            this.tsTopNavigation.Text = "Top Navigation";
            // 
            // tsOpenScript
            // 
            this.tsOpenScript.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsOpenScript.Image = ((System.Drawing.Image)(resources.GetObject("tsOpenScript.Image")));
            this.tsOpenScript.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsOpenScript.Name = "tsOpenScript";
            this.tsOpenScript.Size = new System.Drawing.Size(23, 22);
            this.tsOpenScript.Text = "Open Script";
            this.tsOpenScript.Click += new System.EventHandler(this.tsOpenScript_Click);
            // 
            // tsSaveScript
            // 
            this.tsSaveScript.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsSaveScript.Image = ((System.Drawing.Image)(resources.GetObject("tsSaveScript.Image")));
            this.tsSaveScript.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsSaveScript.Name = "tsSaveScript";
            this.tsSaveScript.Size = new System.Drawing.Size(23, 22);
            this.tsSaveScript.Text = "Save Script";
            this.tsSaveScript.Click += new System.EventHandler(this.tsSaveScript_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsRunScript
            // 
            this.tsRunScript.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsRunScript.Image = ((System.Drawing.Image)(resources.GetObject("tsRunScript.Image")));
            this.tsRunScript.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsRunScript.Name = "tsRunScript";
            this.tsRunScript.Size = new System.Drawing.Size(23, 22);
            this.tsRunScript.Text = "Run Script";
            this.tsRunScript.Click += new System.EventHandler(this.tsRunScript_Click);
            // 
            // tsRunScriptStep
            // 
            this.tsRunScriptStep.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsRunScriptStep.Image = ((System.Drawing.Image)(resources.GetObject("tsRunScriptStep.Image")));
            this.tsRunScriptStep.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsRunScriptStep.Name = "tsRunScriptStep";
            this.tsRunScriptStep.Size = new System.Drawing.Size(23, 22);
            this.tsRunScriptStep.Text = "Run Script Step-by-Step";
            // 
            // tsStop
            // 
            this.tsStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsStop.Image = ((System.Drawing.Image)(resources.GetObject("tsStop.Image")));
            this.tsStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsStop.Name = "tsStop";
            this.tsStop.Size = new System.Drawing.Size(23, 22);
            this.tsStop.Text = "Stop Script";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsCleanScript
            // 
            this.tsCleanScript.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsCleanScript.Image = ((System.Drawing.Image)(resources.GetObject("tsCleanScript.Image")));
            this.tsCleanScript.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsCleanScript.Name = "tsCleanScript";
            this.tsCleanScript.Size = new System.Drawing.Size(23, 22);
            this.tsCleanScript.Text = "Clear/Close Script";
            this.tsCleanScript.Click += new System.EventHandler(this.tsCleanScript_Click);
            // 
            // ofdOpenFile
            // 
            this.ofdOpenFile.FileName = "My MNR Script";
            this.ofdOpenFile.Title = "Open Script";
            // 
            // sfdSaveFile
            // 
            this.sfdSaveFile.Title = "Save Script";
            // 
            // tcScriptContainer
            // 
            this.tcScriptContainer.Controls.Add(this.tpNewScript);
            this.tcScriptContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcScriptContainer.Location = new System.Drawing.Point(0, 0);
            this.tcScriptContainer.Name = "tcScriptContainer";
            this.tcScriptContainer.Padding = new System.Drawing.Point(6, 5);
            this.tcScriptContainer.SelectedIndex = 0;
            this.tcScriptContainer.Size = new System.Drawing.Size(887, 535);
            this.tcScriptContainer.TabIndex = 5;
            // 
            // tpNewScript
            // 
            this.tpNewScript.Controls.Add(this.txtScripts);
            this.tpNewScript.Location = new System.Drawing.Point(4, 26);
            this.tpNewScript.Name = "tpNewScript";
            this.tpNewScript.Padding = new System.Windows.Forms.Padding(3);
            this.tpNewScript.Size = new System.Drawing.Size(879, 505);
            this.tpNewScript.TabIndex = 0;
            this.tpNewScript.Text = "New Script";
            this.tpNewScript.UseVisualStyleBackColor = true;
            // 
            // txtScripts
            // 
            this.txtScripts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtScripts.Location = new System.Drawing.Point(3, 3);
            this.txtScripts.Multiline = true;
            this.txtScripts.Name = "txtScripts";
            this.txtScripts.Size = new System.Drawing.Size(873, 499);
            this.txtScripts.TabIndex = 0;
            // 
            // ScriptRunner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(887, 535);
            this.Controls.Add(this.tsTopNavigation);
            this.Controls.Add(this.tcScriptContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ScriptRunner";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Puppet Master / Script Runner";
            this.TopMost = true;
            this.tsTopNavigation.ResumeLayout(false);
            this.tsTopNavigation.PerformLayout();
            this.tcScriptContainer.ResumeLayout(false);
            this.tpNewScript.ResumeLayout(false);
            this.tpNewScript.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tsTopNavigation;
        private System.Windows.Forms.ToolStripButton tsOpenScript;
        private System.Windows.Forms.ToolStripButton tsRunScript;
        private System.Windows.Forms.ToolStripButton tsRunScriptStep;
        private System.Windows.Forms.ToolStripButton tsStop;
        private System.Windows.Forms.ToolStripButton tsCleanScript;
        private System.Windows.Forms.ToolStripButton tsSaveScript;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.OpenFileDialog ofdOpenFile;
        private System.Windows.Forms.SaveFileDialog sfdSaveFile;
        private System.Windows.Forms.TabControl tcScriptContainer;
        private System.Windows.Forms.TabPage tpNewScript;
        private System.Windows.Forms.TextBox txtScripts;
    }
}

