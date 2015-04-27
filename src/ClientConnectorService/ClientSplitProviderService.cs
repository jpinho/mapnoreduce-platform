using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SharedTypes;

namespace ClientServices
{
	public class ClientSplitProviderService : MarshalByRefObject, IClientSplitProviderService
	{
		private readonly Dictionary<string, List<string>> splitsStore = new Dictionary<string, List<string>>();

		public ClientSplitProviderService() {
		}

		public string GetFileSplit(string filePath, int splitNumber) {
			return splitsStore[filePath][splitNumber - 1];
		}

		public void SplitAndSave(string filePath, int nSplits) {
			var lstSplits = new List<string>();
			var sr = new StreamReader(Path.GetFullPath(filePath));

			var lines = sr.ReadToEnd().Split('\n');
			var splitSize = (int)Math.Floor((double)lines.Length / (double)nSplits);

			// performs the attributions of lines to their splits
			var splitContent = new List<string>();
			foreach (var t in lines) {
				// adds the line to the current split
				splitContent.Add(t);

				// split size reached
				if (splitContent.Count < splitSize)
					continue;

				// saves the split
				lstSplits.Add(string.Join("\n", splitContent));
				// the next cycle is the new split
				splitContent = new List<string>();
			}

			// adds last split (if incomplete)
			if (splitContent.Count >= 0)
				lstSplits.Add(string.Join("\n", splitContent));

			foreach (var split in lstSplits)
				Trace.WriteLine(split);

			// saves the splits of the file on the store
			splitsStore.Add(filePath, lstSplits);
		}
	}
}