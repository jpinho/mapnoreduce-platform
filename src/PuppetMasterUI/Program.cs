using System;
using System.Threading;
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
			new Thread(delegate() {
				//Trace.WriteLine("Starting Puppet Master Service, within few seconds.");
				PlatformCore.PuppetMasterService.Run();
			}).Start();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new ScriptRunner());
		}
	}
}