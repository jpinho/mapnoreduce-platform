using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharedTypes;

namespace ClientServices
{
	public class ClientOutputReceiverService : MarshalByRefObject, IClientOutputReceiverService
	{
		[Serializable]
		public class MapResultEventArgs
		{
			public List<string[]> Result { get; set; }
			public string FilePath { get; set; }
		}

		private readonly Dictionary<string, List<KeyValuePair<int, string[]>>> mapResultStore
			= new Dictionary<string, List<KeyValuePair<int, string[]>>>();
		private EventHandler<MapResultEventArgs> mapResultReadyHandle;
		public event EventHandler<MapResultEventArgs> MapResultReady;

		protected void OnMapResultReady(string filePath, List<string[]> result) {
			if (MapResultReady == null)
				return;
			MapResultReady(this, new MapResultEventArgs() { FilePath = filePath, Result = result });
		}

		public ClientOutputReceiverService() {
		}

		/// <summary>
		/// Overrides the default object leasing behavior such that the object is kept in memory as
		/// long as the host application domain is running.
		/// </summary>
		/// <see><cref>https://msdn.microsoft.com/en-us/library/ms973841.aspx</cref></see>
		public override object InitializeLifetimeService() {
			return null;
		}

		public void ReceiveMapOutputFragment(string filePath, string[] result, int splitNumber) {
			lock (mapResultStore) {
				if (!mapResultStore.ContainsKey(filePath))
					mapResultStore.Add(filePath, new List<KeyValuePair<int, string[]>>());

				mapResultStore[filePath].Add(new KeyValuePair<int, string[]>(splitNumber, result));
				OnMapResultReady(filePath, GetMapResult(filePath));
			}
		}

		public void SubscribeMapResultComplete(Uri endpointUri) {
			mapResultReadyHandle = (sender, args) => {
				try {
					var listener = RemotingHelper.GetRemoteObject<OutputReadyListener>(endpointUri);
					listener.SignalJobProgress(args);
				} catch (Exception ex) {
					Trace.WriteLine("MapResultReady event handler failed to send status update to OutputListener. Error was '"
						+ ex.Message + "'. Subscription cancelled!");
					MapResultReady -= mapResultReadyHandle;
				}
			};
			MapResultReady += mapResultReadyHandle;
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