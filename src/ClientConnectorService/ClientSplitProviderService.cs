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
		private static Guid _clientId;

		public ClientSplitProviderService() {
		}

		/// <summary>
		/// Overrides the default object leasing behavior such that the object is kept in memory as
		/// long as the host application domain is running.
		/// </summary>
		/// <see><cref>https://msdn.microsoft.com/en-us/library/ms973841.aspx</cref></see>
		public override Object InitializeLifetimeService() {
			return null;
		}

		public string GetFileSplit(string filePath, int splitNumber) {
			string split = null;
			Trace.WriteLine("Trying to get split <" + _clientId.ToString() + "," + (splitNumber - 1) + ">");
			try {
				split = splitsStore[_clientId.ToString()][splitNumber - 1];
			} catch (Exception e) {
				Trace.WriteLine("Exception in GetFileSplit: " + e.Message + " " + _clientId.ToString());
			}

			return split;
		}

		public void SplitAndSave(string filePath, int nSplits, Guid clientId) {
			_clientId = clientId;
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

			// saves the splits of the file on the store
			splitsStore.Add(_clientId.ToString(), lstSplits);
		}
	}
}