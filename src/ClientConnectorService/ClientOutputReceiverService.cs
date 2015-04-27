using System;
using System.Collections.Generic;
using System.Linq;
using SharedTypes;

namespace ClientServices
{
	internal class ClientOutputReceiverService : MarshalByRefObject, IClientOutputReceiverService
	{
		private readonly Dictionary<string, List<KeyValuePair<int, string[]>>> mapResultStore
			= new Dictionary<string, List<KeyValuePair<int, string[]>>>();

		public ClientOutputReceiverService() {
		}

		public void ReceiveMapOutputFragment(string filePath, string[] result, int splitNumber) {
			lock (mapResultStore) {
				if (!mapResultStore.ContainsKey(filePath))
					mapResultStore.Add(filePath, new List<KeyValuePair<int, string[]>>());

				mapResultStore[filePath].Add(new KeyValuePair<int, string[]>(splitNumber, result));
				//TODO: trigger event to notify the receival of a new result from a file and to alert that the worker finished that split.
			}
		}

		public List<string[]> GetMapResult(string filePath) {
			lock (mapResultStore) {
				if (!mapResultStore.ContainsKey(filePath))
					return null;

				return (
					from r in mapResultStore[filePath]
					orderby r.Key ascending
					select r.Value
				).ToList();
			}
		}

		public bool IsMapResultReady(string filePath, int nSplits) {
			lock (mapResultStore) {
				var notContainsKey = !mapResultStore.ContainsKey(filePath);
				var isNotYetComplete = mapResultStore.ContainsKey(filePath) && mapResultStore[filePath].Count < nSplits;

				// if both 'false' the result is ready.
				return (!notContainsKey && !isNotYetComplete);
			}
		}
	}
}