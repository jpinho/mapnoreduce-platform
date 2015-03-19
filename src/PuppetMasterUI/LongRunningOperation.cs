using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMasterUI
{
    public partial class LongRunningOperation : Form
    {
        const string EXECUTING_OPERATION = "Operation {0} of {1}: '{2}'";
        int currentOperation = 0;

        public int OperationsCount { get; set; }

        public LongRunningOperation() {
            InitializeComponent();
        }

        public LongRunningOperation(int operationsCount) {
            InitializeComponent();
            OperationsCount = operationsCount;
        }

        public void ReportProgress(string operation) {
            ReportProgress(operation, false);
        }

        public void ReportProgress(string operation, bool incOperationNumber) {
            if (incOperationNumber) ++currentOperation;
            lblOperationStatus.Visible = true;
            lblOperationStatus.Text = string.Format(EXECUTING_OPERATION, currentOperation, OperationsCount, operation);
            pbOperationStatus.Value = ((int)Math.Round(((double)currentOperation / (double)OperationsCount) * 100.0, 0) % 101);
            txtLog.Text += lblOperationStatus.Text + Environment.NewLine;
            txtLog.Select(txtLog.Text.Length - 1, 1);
            txtLog.ScrollToCaret();

            if (currentOperation == OperationsCount) {
                pbOperationStatus.Value = 100;
                btnAbort.Visible = false;
                btnClose.Visible = true;
            }
        }

        private void btnAbort_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Abort;
        }

        private void btnClose_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
