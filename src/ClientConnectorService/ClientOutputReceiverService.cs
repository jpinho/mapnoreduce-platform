using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharedTypes;

namespace ClientServices
{
	internal class ClientOutputReceiverService : MarshalByRefObject, IClientOutputReceiverService
	{
		private readonly Dictionary<string, List<KeyValuePair<int, string[]>>> mapResultStore
			= new Dictionary<string, List<KeyValuePair<int, string[]>>>();

		private readonly Dictionary<string, ManualResetEvent> waitResultEvents = new Dictionary<string, ManualResetEvent>();

		public ClientOutputReceiverService() {
		}

		/// <summary>
		/// Overrides the default object leasing behavior such that the object is kept in memory as
		/// long as the host application domain is running.
		/// </summary>
		/// <see><cref>https://msdn.microsoft.com/en-us/library/ms973841.aspx</cref></see>
		public override Object InitializeLifetimeService() {
			return null;
		}

		public void ReceiveMapOutputFragment(string filePath, string[] result, int splitNumber) {
			lock (mapResultStore) {
				if (!mapResultStore.ContainsKey(filePath))
					mapResultStore.Add(filePath, new List<KeyValuePair<int, string[]>>());

				mapResultStore[filePath].Add(new KeyValuePair<int, string[]>(splitNumber, result));
				waitResultEvents[filePath].Set();
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

		public async Task<List<string[]>> GetMapResultAsync(string filePath, int nSplits) {
			if (IsMapResultReady(filePath, nSplits))
				return GetMapResult(filePath);

			if (!waitResultEvents.ContainsKey(filePath))
				waitResultEvents[filePath] = new ManualResetEvent(false);

			while (!IsMapResultReady(filePath, nSplits)) {
				waitResultEvents[filePath].WaitOne();
			}

			return GetMapResult(filePath);
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