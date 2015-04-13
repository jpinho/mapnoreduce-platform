using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using PlatformCore;
using PuppetMasterLib;
using SharedTypes;

namespace PuppetMasterUI
{
	public partial class ScriptRunner : Form
	{
		private readonly string newScriptTab;
		private readonly PuppetMasterService puppetMasterService;

		public ScriptRunner() {
			InitializeComponent();
			newScriptTab = tpNewScript.Name;
			ofdOpenFile.InitialDirectory = Path.Combine(Environment.CurrentDirectory, "Scripts");
			sfdSaveFile.InitialDirectory = Path.Combine(Environment.CurrentDirectory, "Scripts");
			bwScriptWorker.DoWork += ScriptWorker_DoWork;

			puppetMasterService = RemotingHelper.GetRemoteObject<PuppetMasterService>(PuppetMasterService.ServiceUrl);
			tmrMonitoring.Start();
		}

		private TextBox GetCurrentTextBox() {
			return tcScriptContainer.SelectedTab.Controls[0] as TextBox;
		}

		private void RunScript(bool stepByStep) {
			tsRunScript.Enabled = false;
			tsRunScriptStep.Enabled = false;

			var operationStatus = new LongRunningOperation(stepByStep);
			bwScriptWorker.RunWorkerAsync(new Tuple<LongRunningOperation, string>(
				operationStatus,
				((TextBox)tcScriptContainer.SelectedTab.Controls[0]).Text));

			try {
				operationStatus.ShowDialog();
			} catch (Exception ex) {
				var exShow = ex;

				if (ex.InnerException != null)
					exShow = ex.InnerException;

				MessageBox.Show(exShow.GetType().FullName + " - " + exShow.Message
					+ " -->> " + exShow.StackTrace);
			}

			tsRunScript.Enabled = true;
			tsRunScriptStep.Enabled = true;
		}

		private void ScriptWorker_DoWork(object sender, DoWorkEventArgs e) {
			var state = e.Argument as Tuple<LongRunningOperation, string>;

			if (state == null)
				return;

			var script = state.Item2;
			List<ICommand> commands;
			var operationStatus = state.Item1;
			var errorsCount = 0;

			while (!operationStatus.IsHandleCreated)
				Thread.Sleep(/*medium*/ 500);

			try {
				commands = CommandParser.Run(script, puppetMasterService);
			} catch (Exception ex) {
				operationStatus.Invoke(new MethodInvoker(() => {
					MessageBox.Show("Error while processing script - " + ex.Message + " -->> " + ex.StackTrace);
					operationStatus.DialogResult = DialogResult.Abort;
				}));
				return;
			}
			operationStatus.Invoke(new MethodInvoker(() => operationStatus.OperationsCount = commands.Count));

			if (!operationStatus.SteppedOperation) {
				foreach (var cmd in commands) {
					var operation = cmd.ToString().ToUpper();

					try {
						operationStatus.Invoke(new MethodInvoker(
							() => operationStatus.ReportProgress("Executing '" + operation + "' command...", true)));

						cmd.Execute();
						Thread.Sleep(/*fast*/ 200);

						operationStatus.Invoke(new MethodInvoker(
							() => operationStatus.ReportProgress(operation + " executed successfully!")));
					} catch (Exception ex) {
						errorsCount++;

						try {
							operationStatus.Invoke(new MethodInvoker(() => {
								operationStatus.ReportProgress(
									operation + " failed due to an error '" + ex.Message + "'.");
							}));
						} catch {
							// window handle not created.
						}
					}
				}
				operationStatus.Invoke(new MethodInvoker(
					() => operationStatus.ReportProgress("Script completed" + (errorsCount > 0 ? ", with errors!" : "."))));
			} else {
				var qCommands = new Queue<ICommand>(commands);
				operationStatus.ExecuteNextCommand += (src, args) => {
					var cmd = qCommands.Dequeue();
					var operation = cmd.ToString().ToUpper();

					try {
						operationStatus.Invoke(
							new MethodInvoker(
								() => operationStatus.ReportProgress("Executing '" + operation + "' command...", true)));

						cmd.Execute();
						Thread.Sleep(/*fast*/ 200);

						operationStatus.Invoke(
							new MethodInvoker(
								() => operationStatus.ReportProgress(operation + " executed successfully!")));
					} catch (Exception ex) {
						errorsCount++;
						operationStatus.Invoke(
							new MethodInvoker(
								() => operationStatus.ReportProgress(operation + " failed due to an error '" + ex.Message + "'.")));
					} finally {
						if (qCommands.Count == 0)
							operationStatus.Invoke(
								new MethodInvoker(
									() => operationStatus.ReportProgress("Script completed" + (errorsCount > 0 ? ", with errors!" : "."))));
					}
				};
			}
		}

		private void tsCleanScript_Click(object sender, EventArgs e) {
			if (tcScriptContainer.SelectedTab == tpMonitoring)
				return;
			if (tcScriptContainer.SelectedTab != null && tcScriptContainer.SelectedTab.Name != newScriptTab) {
				tcScriptContainer.TabPages.Remove(tcScriptContainer.SelectedTab);
				tcScriptContainer.SelectedIndex = tcScriptContainer.TabPages.Count - 1;
				return;
			}
			txtScripts.Text = string.Empty;
		}

		private void tsFreezeC_Click(object sender, EventArgs e) {
			GetCurrentTextBox().Text += "FREEZEC <ID>";
		}

		private void tsmiFreezeW_Click(object sender, EventArgs e) {
			GetCurrentTextBox().Text += "FREEZEW <ID>";
		}

		private void tsmiSlowW_Click(object sender, EventArgs e) {
			GetCurrentTextBox().Text += "SLOWW <ID> <delay-in-seconds>";
		}

		private void tsmiStatus_Click(object sender, EventArgs e) {
			GetCurrentTextBox().Text += "STATUS";
		}

		private void tsmiSubmit_Click(object sender, EventArgs e) {
			GetCurrentTextBox().Text += "SUBMIT <ENTRY-URL> <FILE> <OUTPUT> <S> <MAP> <DLL>";
		}

		private void tsmiUnFreezeW_Click(object sender, EventArgs e) {
			GetCurrentTextBox().Text += "UNFREEZEW <ID>";
		}

		private void tsmiWait_Click(object sender, EventArgs e) {
			GetCurrentTextBox().Text += "WAIT <SECS>";
		}

		private void tsmiWorker_Click(object sender, EventArgs e) {
			GetCurrentTextBox().Text += "WORKER <ID> <PUPPETMASTER-URL> <SERVICE-URL> <ENTRY-URL>";
		}

		private void tsOpenScript_Click(object sender, EventArgs e) {
			if (ofdOpenFile.ShowDialog() != DialogResult.OK)
				return;
			using (var fileScript = new StreamReader(ofdOpenFile.OpenFile())) {
				var tpScript = new TabPage(ofdOpenFile.SafeFileName) { Name = ofdOpenFile.SafeFileName };

				var txtScript = new TextBox() {
					Text = fileScript.ReadToEnd(),
					Dock = txtScripts.Dock,
					Multiline = txtScripts.Multiline,
					ScrollBars = txtScripts.ScrollBars,
					Font = txtScripts.Font,
					ForeColor = txtScripts.ForeColor,
					BackColor = txtScripts.BackColor,
					Anchor = txtScripts.Anchor,
					CharacterCasing = txtScripts.CharacterCasing,
					AcceptsReturn = txtScripts.AcceptsReturn,
					AcceptsTab = txtScripts.AcceptsTab,
					AccessibleRole = AccessibleRole.Text
				};

				tpScript.Controls.Add(txtScript);
				tcScriptContainer.TabPages.Add(tpScript);
				tcScriptContainer.SelectedTab = tpScript;
			}
		}

		private void tsRunScript_Click(object sender, EventArgs e) {
			RunScript(false);
		}

		private void tsRunScriptStep_Click(object sender, EventArgs e) {
			RunScript(true);
		}

		private void tsSaveScript_Click(object sender, EventArgs e) {
			if (tcScriptContainer.SelectedTab != null && tcScriptContainer.SelectedTab.Name == newScriptTab) {
				sfdSaveFile.FileName = "Script_" + DateTime.Now.ToString("ddMMyyyy_HHmmss") + ".txt";
				if (sfdSaveFile.ShowDialog() != DialogResult.OK)
					return;
				using (var fileNewScript = new StreamWriter(sfdSaveFile.FileName)) {
					fileNewScript.Write(txtScripts.Text);
				}
			} else if (tcScriptContainer.SelectedTab != null) {
				sfdSaveFile.FileName = tcScriptContainer.SelectedTab.Name;
				if (sfdSaveFile.ShowDialog() != DialogResult.OK)
					return;
				using (var fileNewScript = new StreamWriter(sfdSaveFile.FileName)) {
					fileNewScript.Write(((TextBox)tcScriptContainer.SelectedTab.Controls[0]).Text);
				}
			}
		}

		private void tsUnFreezeC_Click(object sender, EventArgs e) {
			GetCurrentTextBox().Text += "UNFREEZEC <ID>";
		}

		private void tsDdbMonitoring_CheckedChanged(object sender, EventArgs e) {
			if (!tsDdbMonitoring.Checked) {
				tcScriptContainer.TabPages.Remove(tpMonitoring);
				return;
			}

			if (tcScriptContainer.TabPages.Contains(tpMonitoring)) {
				tcScriptContainer.SelectTab(tpMonitoring);
				return;
			}

			tcScriptContainer.TabPages.Add(tpMonitoring);
			tcScriptContainer.SelectTab(tpMonitoring);
		}

		private async void tmrMonitoring_Tick(object sender, EventArgs e) {
			if (cbLiveUpdate.Checked) {
				using (var fs = new FileStream(Path.Combine(Environment.CurrentDirectory, "mnr-trace.log"),
					FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
					using (var reader = new StreamReader(fs)) {
						txtLogFile.Text = await reader.ReadToEndAsync();

						if (cbAutoScroll.Checked) {
							txtLogFile.Select(txtLogFile.Text.Length - 1, 1);
							txtLogFile.ScrollToCaret();
						}
					}
				}
			}

			if (puppetMasterService == null)
				return;

			gvRemoteObjects.Rows.Clear();
			var workers = puppetMasterService.GetWorkers();

			gvRemoteObjects.Rows.Add(new object[] {
				"Puppet Master Service (local)",
				PuppetMasterService.ServiceUrl,
				"Online",
				"Workers #: " + puppetMasterService.GetWorkers().Count
			});
			gvRemoteObjects.Rows[0].MinimumHeight = 25;
			gvRemoteObjects.Rows[0].DefaultCellStyle.BackColor = Color.SaddleBrown;
			gvRemoteObjects.Rows[0].DefaultCellStyle.ForeColor = Color.White;

			foreach (var worker in workers) {
				gvRemoteObjects.Rows.Add(new object[] {
					"Worker Service [ID: " + worker.Value.WorkerId + "]",
					worker.Value.ServiceUrl,
					RemotingHelper.GetRemoteObject<IWorker>(worker.Value.ServiceUrl).GetStatus().ToString(),
					"N/A"
				});
			}
		}

		private void cbMonitoring_CheckedChanged(object sender, EventArgs e) {
			tmrMonitoring.Enabled = cbMonitoring.Checked;
		}

		private void tsDdbMonitoring_Click(object sender, EventArgs e) {

		}
	}
}