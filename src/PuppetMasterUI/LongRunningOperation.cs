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
        const string EXECUTING_OPERATION = "Operation {0} of {1}: Executing '{2}'...";
        int currentOperation = 1;
        int operationsCount = 1;

        public LongRunningOperation() {
            InitializeComponent();
        }

        public LongRunningOperation(int operationsCount) {
            InitializeComponent();
            this.operationsCount = operationsCount;
        }

        public void ReportProgress(string operation) {
            ReportProgress(operation, false);
        }

        public void ReportProgress(string operation, bool incOperationNumber) {
            lblOperationStatus.Visible = true;
            lblOperationStatus.Text = string.Format(EXECUTING_OPERATION, ++currentOperation, operationsCount, operation);
            pbOperationStatus.Value = (int)Math.Round((currentOperation / operationsCount) * 100.0, 0);
        }

        private void btnAbort_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Abort;
        }
    }
}
