using System.Threading;
using PuppetMasterLib.Helpers;
using SharedTypes;

namespace PuppetMasterLib.Commands
{
	public class SubmitJob : ICommand
	{
		public const string NAME = "submit";

		public string EntryUrl { get; set; }
		public string FilePath { get; set; }
		public string OutputPath { get; set; }
		public int Splits { get; set; }
		public string MapClassName { get; set; }
		public string AssemblyFilePath { get; set; }

		public void Execute() {
			new Thread(new ThreadStart(delegate() {
				ConsoleHelper.CreateConsole();
				try {
					UserApplicationSample.Program.ExecuteMapJob(
						EntryUrl, FilePath, OutputPath, Splits,
						MapClassName, AssemblyFilePath);
				} finally {
					ConsoleHelper.FreeConsole();
				}
			})).Start();
		}

		public override string ToString() {
			return NAME;
		}
	}
}