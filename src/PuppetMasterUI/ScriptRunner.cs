using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using SharedTypes;
using PuppetMasterLib.Commands;
using PuppetMasterLib;

namespace PuppetMasterUI
{
    public partial class ScriptRunner : Form
    {
        private readonly string NEW_SCRIPT_TAB;

        public ScriptRunner() {
            InitializeComponent();
            NEW_SCRIPT_TAB = tpNewScript.Name;
            ofdOpenFile.InitialDirectory = Path.Combine(Environment.CurrentDirectory, "Scripts");
            sfdSaveFile.InitialDirectory = Path.Combine(Environment.CurrentDirectory, "Scripts");
            bwScriptWorker.DoWork += ScriptWorker_DoWork;
        }

        private void tsOpenScript_Click(object sender, EventArgs e) {
            if (ofdOpenFile.ShowDialog() == DialogResult.OK) {
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
        }

        private void tsCleanScript_Click(object sender, EventArgs e) {
            if (tcScriptContainer.SelectedTab != null && tcScriptContainer.SelectedTab.Name != NEW_SCRIPT_TAB) {
                tcScriptContainer.TabPages.Remove(tcScriptContainer.SelectedTab);
                tcScriptContainer.SelectedIndex = tcScriptContainer.TabPages.Count - 1;
            } else
                txtScripts.Text = string.Empty;
        }

        private void tsSaveScript_Click(object sender, EventArgs e) {
            if (tcScriptContainer.SelectedTab != null && tcScriptContainer.SelectedTab.Name == NEW_SCRIPT_TAB) {
                sfdSaveFile.FileName = "Script_" + DateTime.Now.ToString("ddMMyyyy_HHmmss") + ".txt";
                if (sfdSaveFile.ShowDialog() == DialogResult.OK) {
                    using (StreamWriter fileNewScript = new StreamWriter(sfdSaveFile.FileName)) {
                        fileNewScript.Write(txtScripts.Text);
                    }
                }
            } else {
                sfdSaveFile.FileName = tcScriptContainer.SelectedTab.Name;
                if (sfdSaveFile.ShowDialog() == DialogResult.OK) {
                    using (StreamWriter fileNewScript = new StreamWriter(sfdSaveFile.FileName)) {
                        fileNewScript.Write((tcScriptContainer.SelectedTab.Controls[0] as TextBox).Text);
                    }
                }
            }
        }

        private void tsRunScript_Click(object sender, EventArgs e) {
            RunScript(false);
        }

        private void tsRunScriptStep_Click(object sender, EventArgs e) {
            RunScript(true);
        }

        private void ScriptWorker_DoWork(object sender, DoWorkEventArgs e) {
            Tuple<LongRunningOperation, string> state = e.Argument as Tuple<LongRunningOperation, string>;
            string script = state.Item2;
            List<ICommand> commands = null;
            LongRunningOperation operationStatus = state.Item1;
            int errorsCount = 0;

            while (!operationStatus.IsHandleCreated)
                Thread.Sleep(500);

            try {
                commands = CommandParser.Run(script);
            } catch (Exception ex) {
                operationStatus.Invoke(new MethodInvoker(delegate() {
                    MessageBox.Show("Error while processing script - " + ex.Message + " -->> " + ex.StackTrace);
                }));
                operationStatus.Invoke(new MethodInvoker(delegate() {
                    operationStatus.DialogResult = System.Windows.Forms.DialogResult.Abort;
                }));
                return;
            }

            operationStatus.Invoke(new MethodInvoker(delegate() {
                operationStatus.OperationsCount = commands.Count;
            }));

            if (!operationStatus.SteppedOperation) {
                foreach (ICommand cmd in commands) {
                    string operation = cmd.ToString().ToUpper();

                    try {
                        operationStatus.Invoke(new MethodInvoker(delegate() {
                            operationStatus.ReportProgress("Executing '" + operation + "' command...", true);
                        }));

                        cmd.Execute();
                        Thread.Sleep(1000);

                        operationStatus.Invoke(new MethodInvoker(delegate() {
                            operationStatus.ReportProgress(operation + " executed successfully!");
                        }));
                    } catch (Exception ex) {
                        errorsCount++;
                        operationStatus.Invoke(new MethodInvoker(delegate() {
                            operationStatus.ReportProgress(operation + " failed due to an error '" + ex.Message + "'.");
                        }));
                    }
                }

                operationStatus.Invoke(new MethodInvoker(delegate() {
                    operationStatus.ReportProgress("Script completed" + (errorsCount > 0 ? ", with errors!" : "."));
                }));
            } else {
                Queue<ICommand> qCommands = new Queue<ICommand>(commands);
                operationStatus.ExecuteNextCommand += new EventHandler(delegate(object src, EventArgs args) {
                    ICommand cmd = qCommands.Dequeue();
                    string operation = cmd.ToString().ToUpper();

                    try {
                        operationStatus.Invoke(new MethodInvoker(delegate() {
                            operationStatus.ReportProgress("Executing '" + operation + "' command...", true);
                        }));

                        cmd.Execute();
                        Thread.Sleep(1000);

                        operationStatus.Invoke(new MethodInvoker(delegate() {
                            operationStatus.ReportProgress(operation + " executed successfully!");
                        }));
                    } catch (Exception ex) {
                        errorsCount++;
                        operationStatus.Invoke(new MethodInvoker(delegate() {
                            operationStatus.ReportProgress(operation + " failed due to an error '" + ex.Message + "'.");
                        }));
                    } finally {
                        if (qCommands.Count == 0) {
                            operationStatus.Invoke(new MethodInvoker(delegate() {
                                operationStatus.ReportProgress("Script completed" + (errorsCount > 0 ? ", with errors!" : "."));
                            }));
                        }
                    }
                });
            }
        }

        private void RunScript(bool stepByStep) {
            tsRunScript.Enabled = false;
            tsRunScriptStep.Enabled = false;

            LongRunningOperation operationStatus = new LongRunningOperation(stepByStep);

            bwScriptWorker.RunWorkerAsync(new Tuple<LongRunningOperation, string>(
                operationStatus,
                (tcScriptContainer.SelectedTab.Controls[0] as TextBox).Text));

            operationStatus.ShowDialog();
            tsRunScript.Enabled = true;
            tsRunScriptStep.Enabled = true;
        }

        private TextBox GetCurrentTextBox() {
            return tcScriptContainer.SelectedTab.Controls[0] as TextBox;
        }

        private void tsmiWorker_Click(object sender, EventArgs e) {
            GetCurrentTextBox().Text += "WORKER <ID> <PUPPETMASTER-URL> <SERVICE-URL> <ENTRY-URL>";
        }

        private void tsmiSubmit_Click(object sender, EventArgs e) {
            GetCurrentTextBox().Text += "SUBMIT <ENTRY-URL> <FILE> <OUTPUT> <S> <MAP> <DLL>";
        }

        private void tsmiWait_Click(object sender, EventArgs e) {
            GetCurrentTextBox().Text += "WAIT <SECS>";
        }

        private void tsmiStatus_Click(object sender, EventArgs e) {
            GetCurrentTextBox().Text += "STATUS";
        }

        private void tsmiSlowW_Click(object sender, EventArgs e) {
            GetCurrentTextBox().Text += "SLOWW <ID> <delay-in-seconds>";
        }

        private void tsmiFreezeW_Click(object sender, EventArgs e) {
            GetCurrentTextBox().Text += "FREEZEW <ID>";
        }

        private void tsmiUnFreezeW_Click(object sender, EventArgs e) {
            GetCurrentTextBox().Text += "UNFREEZEW <ID>";
        }

        private void tsFreezeC_Click(object sender, EventArgs e) {
            GetCurrentTextBox().Text += "FREEZEC <ID>";
        }

        private void tsUnFreezeC_Click(object sender, EventArgs e) {
            GetCurrentTextBox().Text += "UNFREEZEC <ID>";
        }
    }
}