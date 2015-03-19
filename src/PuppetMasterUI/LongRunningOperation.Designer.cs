namespace PuppetMasterUI
{
    partial class LongRunningOperation
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
            this.grpRunStatus = new System.Windows.Forms.GroupBox();
            this.btnAbort = new System.Windows.Forms.Button();
            this.lblOperationStatus = new System.Windows.Forms.Label();
            this.pbOperationStatus = new System.Windows.Forms.ProgressBar();
            this.grpRunStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpRunStatus
            // 
            this.grpRunStatus.Controls.Add(this.btnAbort);
            this.grpRunStatus.Controls.Add(this.lblOperationStatus);
            this.grpRunStatus.Controls.Add(this.pbOperationStatus);
            this.grpRunStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpRunStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpRunStatus.Location = new System.Drawing.Point(5, 5);
            this.grpRunStatus.Name = "grpRunStatus";
            this.grpRunStatus.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.grpRunStatus.Size = new System.Drawing.Size(489, 90);
            this.grpRunStatus.TabIndex = 7;
            this.grpRunStatus.TabStop = false;
            this.grpRunStatus.Text = "Running Script";
            // 
            // btnAbort
            // 
            this.btnAbort.BackColor = System.Drawing.Color.Firebrick;
            this.btnAbort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAbort.Font = new System.Drawing.Font("Tahoma", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAbort.ForeColor = System.Drawing.Color.White;
            this.btnAbort.Location = new System.Drawing.Point(409, 49);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(67, 23);
            this.btnAbort.TabIndex = 10;
            this.btnAbort.Text = "ABORT";
            this.btnAbort.UseVisualStyleBackColor = false;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // lblOperationStatus
            // 
            this.lblOperationStatus.AutoSize = true;
            this.lblOperationStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOperationStatus.Location = new System.Drawing.Point(12, 23);
            this.lblOperationStatus.Name = "lblOperationStatus";
            this.lblOperationStatus.Size = new System.Drawing.Size(256, 13);
            this.lblOperationStatus.TabIndex = 9;
            this.lblOperationStatus.Text = "Operation 1 of 10: Executing command \'WORKER\'...";
            this.lblOperationStatus.Visible = false;
            // 
            // pbOperationStatus
            // 
            this.pbOperationStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.pbOperationStatus.Location = new System.Drawing.Point(15, 49);
            this.pbOperationStatus.Name = "pbOperationStatus";
            this.pbOperationStatus.Size = new System.Drawing.Size(384, 23);
            this.pbOperationStatus.TabIndex = 8;
            // 
            // LongRunningOperation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 100);
            this.ControlBox = false;
            this.Controls.Add(this.grpRunStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "LongRunningOperation";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LongRunningOperation";
            this.TopMost = true;
            this.grpRunStatus.ResumeLayout(false);
            this.grpRunStatus.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpRunStatus;
        private System.Windows.Forms.Label lblOperationStatus;
        private System.Windows.Forms.ProgressBar pbOperationStatus;
        private System.Windows.Forms.Button btnAbort;
    }
}