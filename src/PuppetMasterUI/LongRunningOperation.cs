using System;
using System.Windows.Forms;

namespace PuppetMasterUI
{
	public partial class LongRunningOperation : Form
	{
		private const string EXECUTING_OPERATION = "Operation {0} of {1}: '{2}'";
		private int currentOperation = 0;
		public event EventHandler ExecuteNextCommand;

		public int OperationsCount { get; set; }
		public bool SteppedOperation { get; set; }

		public LongRunningOperation() {
			InitializeComponent();
		}

		public LongRunningOperation(bool steppedOperation)
			: this() {
			this.SteppedOperation = steppedOperation;
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
			if (OperationsCount > 0)
				pbOperationStatus.Value = ((int)Math.Round(((double)currentOperation / (double)OperationsCount) * 100.0, 0) % 101);
			txtLog.Text += "[" + DateTime.Now.ToString("dd/MM/yy HH:mm:ss") + "] " + lblOperationStatus.Text + Environment.NewLine;
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
			this.DialogResult = DialogResult.Abort;
		}

		private void btnClose_Click(object sender, EventArgs e) {
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
		}

		private void btnRunStep_Click(object sender, EventArgs e) {
			OnExecuteNextCommand();
		}
	}
}