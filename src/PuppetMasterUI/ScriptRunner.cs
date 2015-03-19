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
        }

        private void tsOpenScript_Click(object sender, EventArgs e) {
            if (ofdOpenFile.ShowDialog() == DialogResult.OK) {
                StreamReader fileScript = new StreamReader(ofdOpenFile.OpenFile());
                TabPage tpScript = new TabPage(ofdOpenFile.SafeFileName) { Name = ofdOpenFile.SafeFileName };

                TextBox txtScript = new TextBox() {
                    Text = fileScript.ReadToEnd(),
                    Dock = DockStyle.Fill,
                    Multiline = true,
                    ScrollBars = ScrollBars.Both
                };

                tpScript.Controls.Add(txtScript);
                tcScriptContainer.TabPages.Add(tpScript);
                tcScriptContainer.SelectedTab = tpScript;
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
            if (tcScriptContainer.SelectedTab != null && tcScriptContainer.SelectedTab.Name == NEW_SCRIPT_TAB){
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
            new LongRunningOperation().ShowDialog();
        }
    }   
}