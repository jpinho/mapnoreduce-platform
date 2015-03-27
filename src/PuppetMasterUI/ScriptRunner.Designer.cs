namespace PuppetMasterUI
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
            this.tsSeparatorOne = new System.Windows.Forms.ToolStripSeparator();
            this.tsRunScript = new System.Windows.Forms.ToolStripButton();
            this.tsRunScriptStep = new System.Windows.Forms.ToolStripButton();
            this.tsCleanScript = new System.Windows.Forms.ToolStripButton();
            this.tsSeparatorTwo = new System.Windows.Forms.ToolStripSeparator();
            this.tsDdbCommandHelper = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsmiWorker = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSubmit = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiWait = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiStatus = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSlowW = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiFreezeW = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiUnFreezeW = new System.Windows.Forms.ToolStripMenuItem();
            this.tsFreezeC = new System.Windows.Forms.ToolStripMenuItem();
            this.tsUnFreezeC = new System.Windows.Forms.ToolStripMenuItem();
            this.ofdOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.sfdSaveFile = new System.Windows.Forms.SaveFileDialog();
            this.tcScriptContainer = new System.Windows.Forms.TabControl();
            this.tpNewScript = new System.Windows.Forms.TabPage();
            this.txtScripts = new System.Windows.Forms.TextBox();
            this.bwScriptWorker = new System.ComponentModel.BackgroundWorker();
            this.tsDdbMonitoring = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
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
            this.tsSeparatorOne,
            this.tsRunScript,
            this.tsRunScriptStep,
            this.tsCleanScript,
            this.tsSeparatorTwo,
            this.tsDdbCommandHelper,
            this.toolStripSeparator1,
            this.tsDdbMonitoring});
            this.tsTopNavigation.Location = new System.Drawing.Point(0, 0);
            this.tsTopNavigation.Name = "tsTopNavigation";
            this.tsTopNavigation.Padding = new System.Windows.Forms.Padding(0);
            this.tsTopNavigation.Size = new System.Drawing.Size(878, 25);
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
            // tsSeparatorOne
            // 
            this.tsSeparatorOne.Name = "tsSeparatorOne";
            this.tsSeparatorOne.Size = new System.Drawing.Size(6, 25);
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
            this.tsRunScriptStep.Click += new System.EventHandler(this.tsRunScriptStep_Click);
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
            // tsSeparatorTwo
            // 
            this.tsSeparatorTwo.Name = "tsSeparatorTwo";
            this.tsSeparatorTwo.Size = new System.Drawing.Size(6, 25);
            // 
            // tsDdbCommandHelper
            // 
            this.tsDdbCommandHelper.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsDdbCommandHelper.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiWorker,
            this.tsmiSubmit,
            this.tsmiWait,
            this.tsmiStatus,
            this.tsmiSlowW,
            this.tsmiFreezeW,
            this.tsmiUnFreezeW,
            this.tsFreezeC,
            this.tsUnFreezeC});
            this.tsDdbCommandHelper.Image = ((System.Drawing.Image)(resources.GetObject("tsDdbCommandHelper.Image")));
            this.tsDdbCommandHelper.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsDdbCommandHelper.Name = "tsDdbCommandHelper";
            this.tsDdbCommandHelper.Size = new System.Drawing.Size(115, 22);
            this.tsDdbCommandHelper.Text = "Command Helper";
            // 
            // tsmiWorker
            // 
            this.tsmiWorker.Name = "tsmiWorker";
            this.tsmiWorker.Size = new System.Drawing.Size(152, 22);
            this.tsmiWorker.Text = "WORKER";
            this.tsmiWorker.Click += new System.EventHandler(this.tsmiWorker_Click);
            // 
            // tsmiSubmit
            // 
            this.tsmiSubmit.Name = "tsmiSubmit";
            this.tsmiSubmit.Size = new System.Drawing.Size(152, 22);
            this.tsmiSubmit.Text = "SUBMIT";
            this.tsmiSubmit.Click += new System.EventHandler(this.tsmiSubmit_Click);
            // 
            // tsmiWait
            // 
            this.tsmiWait.Name = "tsmiWait";
            this.tsmiWait.Size = new System.Drawing.Size(152, 22);
            this.tsmiWait.Text = "WAIT";
            this.tsmiWait.Click += new System.EventHandler(this.tsmiWait_Click);
            // 
            // tsmiStatus
            // 
            this.tsmiStatus.Name = "tsmiStatus";
            this.tsmiStatus.Size = new System.Drawing.Size(152, 22);
            this.tsmiStatus.Text = "STATUS";
            this.tsmiStatus.Click += new System.EventHandler(this.tsmiStatus_Click);
            // 
            // tsmiSlowW
            // 
            this.tsmiSlowW.Name = "tsmiSlowW";
            this.tsmiSlowW.Size = new System.Drawing.Size(152, 22);
            this.tsmiSlowW.Text = "SLOWW";
            this.tsmiSlowW.Click += new System.EventHandler(this.tsmiSlowW_Click);
            // 
            // tsmiFreezeW
            // 
            this.tsmiFreezeW.Name = "tsmiFreezeW";
            this.tsmiFreezeW.Size = new System.Drawing.Size(152, 22);
            this.tsmiFreezeW.Text = "FREEZEW";
            this.tsmiFreezeW.Click += new System.EventHandler(this.tsmiFreezeW_Click);
            // 
            // tsmiUnFreezeW
            // 
            this.tsmiUnFreezeW.Name = "tsmiUnFreezeW";
            this.tsmiUnFreezeW.Size = new System.Drawing.Size(152, 22);
            this.tsmiUnFreezeW.Text = "UNFREEZEW";
            this.tsmiUnFreezeW.Click += new System.EventHandler(this.tsmiUnFreezeW_Click);
            // 
            // tsFreezeC
            // 
            this.tsFreezeC.Name = "tsFreezeC";
            this.tsFreezeC.Size = new System.Drawing.Size(152, 22);
            this.tsFreezeC.Text = "FREEZEC";
            this.tsFreezeC.Click += new System.EventHandler(this.tsFreezeC_Click);
            // 
            // tsUnFreezeC
            // 
            this.tsUnFreezeC.Name = "tsUnFreezeC";
            this.tsUnFreezeC.Size = new System.Drawing.Size(152, 22);
            this.tsUnFreezeC.Text = "UNFREEZEC";
            this.tsUnFreezeC.Click += new System.EventHandler(this.tsUnFreezeC_Click);
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
            this.tcScriptContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tcScriptContainer.Controls.Add(this.tpNewScript);
            this.tcScriptContainer.Location = new System.Drawing.Point(0, 26);
            this.tcScriptContainer.Name = "tcScriptContainer";
            this.tcScriptContainer.Padding = new System.Drawing.Point(6, 5);
            this.tcScriptContainer.SelectedIndex = 0;
            this.tcScriptContainer.Size = new System.Drawing.Size(878, 530);
            this.tcScriptContainer.TabIndex = 5;
            // 
            // tpNewScript
            // 
            this.tpNewScript.Controls.Add(this.txtScripts);
            this.tpNewScript.Location = new System.Drawing.Point(4, 26);
            this.tpNewScript.Name = "tpNewScript";
            this.tpNewScript.Padding = new System.Windows.Forms.Padding(3);
            this.tpNewScript.Size = new System.Drawing.Size(870, 500);
            this.tpNewScript.TabIndex = 0;
            this.tpNewScript.Text = "New Script";
            this.tpNewScript.UseVisualStyleBackColor = true;
            // 
            // txtScripts
            // 
            this.txtScripts.AcceptsReturn = true;
            this.txtScripts.AcceptsTab = true;
            this.txtScripts.AccessibleRole = System.Windows.Forms.AccessibleRole.Text;
            this.txtScripts.AllowDrop = true;
            this.txtScripts.AutoCompleteCustomSource.AddRange(new string[] {
            "WORKER",
            "SUBMIT",
            "WAIT",
            "SLOWW",
            "STATUS",
            "FREEZEW",
            "UNFREEZEW",
            "FREEZEC",
            "UNFREEZEC"});
            this.txtScripts.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtScripts.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtScripts.BackColor = System.Drawing.Color.Black;
            this.txtScripts.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtScripts.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtScripts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtScripts.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtScripts.ForeColor = System.Drawing.Color.Lavender;
            this.txtScripts.HideSelection = false;
            this.txtScripts.Location = new System.Drawing.Point(3, 3);
            this.txtScripts.Multiline = true;
            this.txtScripts.Name = "txtScripts";
            this.txtScripts.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtScripts.Size = new System.Drawing.Size(864, 494);
            this.txtScripts.TabIndex = 0;
            this.txtScripts.Text = "% TYPE YOUR SCRIPT HERE!";
            // 
            // bwScriptWorker
            // 
            this.bwScriptWorker.WorkerReportsProgress = true;
            this.bwScriptWorker.WorkerSupportsCancellation = true;
            // 
            // tsDdbMonitoring
            // 
            this.tsDdbMonitoring.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsDdbMonitoring.Image = ((System.Drawing.Image)(resources.GetObject("tsDdbMonitoring.Image")));
            this.tsDdbMonitoring.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsDdbMonitoring.Name = "tsDdbMonitoring";
            this.tsDdbMonitoring.Size = new System.Drawing.Size(71, 22);
            this.tsDdbMonitoring.Text = "Monitoring";
            this.tsDdbMonitoring.Click += new System.EventHandler(this.tsDdbMonitoring_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // ScriptRunner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(878, 557);
            this.Controls.Add(this.tsTopNavigation);
            this.Controls.Add(this.tcScriptContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.IsMdiContainer = true;
            this.Name = "ScriptRunner";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Puppet Master / Script Runner";
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
        private System.Windows.Forms.ToolStripButton tsCleanScript;
        private System.Windows.Forms.ToolStripButton tsSaveScript;
        private System.Windows.Forms.ToolStripSeparator tsSeparatorOne;
        private System.Windows.Forms.ToolStripSeparator tsSeparatorTwo;
        private System.Windows.Forms.OpenFileDialog ofdOpenFile;
        private System.Windows.Forms.SaveFileDialog sfdSaveFile;
        private System.Windows.Forms.TabControl tcScriptContainer;
        private System.Windows.Forms.TabPage tpNewScript;
        private System.Windows.Forms.TextBox txtScripts;
        private System.ComponentModel.BackgroundWorker bwScriptWorker;
        private System.Windows.Forms.ToolStripDropDownButton tsDdbCommandHelper;
        private System.Windows.Forms.ToolStripMenuItem tsmiWorker;
        private System.Windows.Forms.ToolStripMenuItem tsmiSubmit;
        private System.Windows.Forms.ToolStripMenuItem tsmiWait;
        private System.Windows.Forms.ToolStripMenuItem tsmiStatus;
        private System.Windows.Forms.ToolStripMenuItem tsmiSlowW;
        private System.Windows.Forms.ToolStripMenuItem tsmiFreezeW;
        private System.Windows.Forms.ToolStripMenuItem tsmiUnFreezeW;
        private System.Windows.Forms.ToolStripMenuItem tsFreezeC;
        private System.Windows.Forms.ToolStripMenuItem tsUnFreezeC;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsDdbMonitoring;
    }
}

