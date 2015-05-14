using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace PuppetMasterUI
{
    public partial class LongRunningOperation : Form
    {
        private const string EXECUTING_OPERATION = "Operation {0} of {1}: '{2}'";
        private int currentOperation;
        public event EventHandler ExecuteNextCommand;

        public int OperationsCount { get; set; }
        public bool SteppedOperation { get; set; }

        public LongRunningOperation() {
            InitializeComponent();
        }

        public LongRunningOperation(bool steppedOperation)
            : this() {
            SteppedOperation = steppedOperation;
            if (!steppedOperation)
                return;
            pbOperationStatus.Width = 406;
            btnRunStep.Visible = true;
            lblOperationStatus.Text = "Press the 'STEP' button to run one command at a time.";
            lblOperationStatus.Visible = true;
        }

        private void OnExecuteNextCommand() {
            if (ExecuteNextCommand != null && SteppedOperation)
                ExecuteNextCommand(this, EventArgs.Empty);
        }

        public void ReportProgress(string operation) {
            ReportProgress(operation, false);
        }

        public void ReportProgress(string operation, bool incOperationNumber) {
            if (incOperationNumber)
                ++currentOperation;

            lblOperationStatus.Visible = true;
            lblOperationStatus.Text = string.Format(EXECUTING_OPERATION, currentOperation, OperationsCount, operation);

            if (OperationsCount > 0) {
                // ReSharper disable RedundantCast
                pbOperationStatus.Value = ((int)Math.Round(((double)currentOperation / (double)OperationsCount) * 100.0, 0) % 101);
                // ReSharper restore RedundantCast
            }

            txtLog.Text += "[" + DateTime.Now.ToString("dd/MM/yy HH:mm:ss") + "] " + lblOperationStatus.Text + Environment.NewLine;
            Trace.WriteLine(txtLog.Text);

            txtLog.Select(txtLog.Text.Length - 1, 1);
            txtLog.ScrollToCaret();

            if (currentOperation != OperationsCount)
                return;

            pbOperationStatus.Value = 100;
            btnAbort.Visible = false;
            btnClose.Visible = true;
            btnRunStep.Enabled = false;
        }

        private void btnAbort_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Abort;
        }

        private void btnClose_Click(object sender, EventArgs e) {
            if (Modal)
                DialogResult = DialogResult.OK;
            else
                Close();
        }

        private void btnRunStep_Click(object sender, EventArgs e) {
            OnExecuteNextCommand();
        }
    }
}