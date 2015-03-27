using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMasterUI
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {
            new Thread(new ThreadStart(delegate() {
                Debug.WriteLine("Starting Puppe tMaster Service... give it some seconds to start.");
                PlatformCore.PuppetMasterService.Run();
            })).Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ScriptRunner());
        }
    }
}