using System;
using System.Diagnostics;
using ClientServices;

namespace UserApplicationSample
{
	public class Program
	{
		private static readonly string HelpMessage = Resources.HelpMessage;

		public static void Main(string[] args) {
			if (args.Length == 1)
				Console.Out.Write(HelpMessage);

			var entryUrl = args[1];
			var file = args[2];
			var ouput = args[3];
			var nSplits = Int32.Parse(args[4]);
			var mapClassName = args[5];
			var assemblyFilePath = args[6];

			ExecuteMapJob(entryUrl, file, ouput, nSplits, mapClassName, assemblyFilePath);

			Console.ReadKey();
		}

		public static void ExecuteMapJob(string entryUrl, string filePath, string outputPath, int splits, string mapClassName, string assemblyFilePath) {
			Console.WriteLine(Resources.USER_APP_LOG, entryUrl, filePath, outputPath, splits, mapClassName, assemblyFilePath);
			Trace.WriteLine(string.Format(Resources.USER_APP_LOG, entryUrl, filePath, outputPath, splits, mapClassName, assemblyFilePath));

			var client = new ClientService();
			client.Init(entryUrl);

			client.Submit(filePath, splits, outputPath, mapClassName, assemblyFilePath);
			Console.WriteLine(Resources.JOB_SUBMIT_WAIT);
			Trace.WriteLine(Resources.JOB_SUBMIT_WAIT);
		}
	}
}