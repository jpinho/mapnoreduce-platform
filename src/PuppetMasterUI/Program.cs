using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PlatformServer;
using System.Threading;

namespace PuppetMasterUI
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ScriptRunner());

            new Thread(new ThreadStart(delegate()
            {
                PlatformServer.Program.Start();
            })).Start();
        }
    }
}
