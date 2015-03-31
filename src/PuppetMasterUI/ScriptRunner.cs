using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using PlatformCore;
using PuppetMasterLib;
using PuppetMasterUI.Properties;
using SharedTypes;

namespace PuppetMasterUI
{
    public partial class ScriptRunner : Form
    {
        public ScriptRunner() {
            InitializeComponent();
            newScriptTab = tpNewScript.Name;
            ofdOpenFile.InitialDirectory = Path.Combine(Environment.CurrentDirectory, "Scripts");
            sfdSaveFile.InitialDirectory = Path.Combine(Environment.CurrentDirectory, "Scripts");
            bwScriptWorker.DoWork += ScriptWorker_DoWork;
        }

        private readonly string newScriptTab;

        private TextBox GetCurrentTextBox() {
            return tcScriptContainer.SelectedTab.Controls[0] as TextBox;
        }

        private void RunScript(bool stepByStep) {
            tsRunScript.Enabled = false;
            tsRunScriptStep.Enabled = false;

            LongRunningOperation operationStatus = new LongRunningOperation(stepByStep);

            bwScriptWorker.RunWorkerAsync(new Tuple<LongRunningOperation, string>(
                operationStatus,
                ((TextBox)tcScriptContainer.SelectedTab.Controls[0]).Text));

            operationStatus.ShowDialog();
            tsRunScript.Enabled = true;
            tsRunScriptStep.Enabled = true;
        }

        private void ScriptWorker_DoWork(object sender, DoWorkEventArgs e) {
            var state = e.Argument as Tuple<LongRunningOperation, string>;
            var script = state.Item2;
            List<ICommand> commands;
            var operationStatus = state.Item1;
            var errorsCount = 0;

            while (!operationStatus.IsHandleCreated)
                Thread.Sleep(500);

            try {
                commands = CommandParser.Run(script);
            } catch (Exception ex) {
                operationStatus.Invoke(new MethodInvoker(delegate {
                    MessageBox.Show("Error while processing script - " + ex.Message + " -->> " + ex.StackTrace);
                    operationStatus.DialogResult = DialogResult.Abort;
                }));
                return;
            }

            operationStatus.Invoke(new MethodInvoker(delegate {
                operationStatus.OperationsCount = commands.Count;
            }));

            if (!operationStatus.SteppedOperation) {
                foreach (var cmd in commands) {
                    var operation = cmd.ToString().ToUpper();

                    try {
                        operationStatus.Invoke(new MethodInvoker(delegate {
                            operationStatus.ReportProgress("Executing '" + operation + "' command...", true);
                        }));

                        cmd.Execute();
                        Thread.Sleep(1000);

                        operationStatus.Invoke(new MethodInvoker(delegate {
                            operationStatus.ReportProgress(operation + " executed successfully!");
                        }));
                    } catch (Exception ex) {
                        errorsCount++;
                        operationStatus.Invoke(new MethodInvoker(delegate {
                            operationStatus.ReportProgress(operation + " failed due to an error '" + ex.Message + "'.");
                        }));
                    }
                }

                operationStatus.Invoke(new MethodInvoker(delegate {
                    operationStatus.ReportProgress("Script completed" + (errorsCount > 0 ? ", with errors!" : "."));
                }));
            } else {
                var qCommands = new Queue<ICommand>(commands);
                operationStatus.ExecuteNextCommand += (src, args) => {
                    var cmd = qCommands.Dequeue();
                    var operation = cmd.ToString().ToUpper();

                    try {
                        operationStatus.Invoke(
                            new MethodInvoker(
                                delegate {
                                    operationStatus.ReportProgress("Executing '" + operation + "' command...", true);
                                }));

                        cmd.Execute();
                        Thread.Sleep(1000);

                        operationStatus.Invoke(
                            new MethodInvoker(
                                delegate { operationStatus.ReportProgress(operation + " executed successfully!"); }));
                    } catch (Exception ex) {
                        errorsCount++;
                        operationStatus.Invoke(
                            new MethodInvoker(
                                delegate {
                                    operationStatus.ReportProgress(operation + " failed due to an error '" + ex.Message +
                                                                   "'.");
                                }));
                    } finally {
                        if (qCommands.Count == 0) {
                            operationStatus.Invoke(
                                new MethodInvoker(
                                    delegate {
                                        operationStatus.ReportProgress("Script completed" +
                                                                       (errorsCount > 0 ? ", with errors!" : "."));
                                    }));
                        }
                    }
                };
            }
        }

        private void tsCleanScript_Click(object sender, EventArgs e) {
            if (tcScriptContainer.SelectedTab != null && tcScriptContainer.SelectedTab.Name != newScriptTab) {
                tcScriptContainer.TabPages.Remove(tcScriptContainer.SelectedTab);
                tcScriptContainer.SelectedIndex = tcScriptContainer.TabPages.Count - 1;
            } else
                txtScripts.Text = string.Empty;
        }

        private void tsDdbMonitoring_Click(object sender, EventArgs e) {
            var pMaster = (IPuppetMasterService)Activator.GetObject(
                typeof(IPuppetMasterService),
                PuppetMasterService.ServiceUrl.ToString());

            var workerUrLs = pMaster.GetWorkers().Aggregate(string.Empty,
                (current, entry) => current + string.Format("\t\tWorker{0}: [{1}]\n"
                    , entry.Key, entry.Value.ServiceUrl));

            var message = string.Format(
                Resources.PuppetMasterServerStatusMessage.ToUpperInvariant()
                    , PuppetMasterService.ServiceUrl
                    , pMaster.GetWorkers().Count
                    , workerUrLs);

            MessageBox.Show(message, "[~] Puppet Master Server Monitor [~]",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            using (StreamReader fileScript = new StreamReader(ofdOpenFile.OpenFile())) {
                TabPage tpScript = new TabPage(ofdOpenFile.SafeFileName) { Name = ofdOpenFile.SafeFileName };

                TextBox txtScript = new TextBox() {
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
    }
}