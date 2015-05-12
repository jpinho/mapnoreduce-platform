using System;
using System.Collections.Generic;
using System.Threading;
using SharedTypes;

namespace ClientServices
{
	public class OutputReadyListener : MarshalByRefObject
	{
		private readonly ManualResetEvent waitHandle = new ManualResetEvent(false);
		private bool isStarted = false;

		public int NumberOfSplits { get; private set; }
		public List<string[]> Result { get; private set; }
		public Uri ServiceUri { get; private set; }

		public OutputReadyListener(int nJobSplits) {
			ServiceUri = new Uri("tcp://localhost:9090/" + Guid.NewGuid().ToString("N"));
			NumberOfSplits = nJobSplits;
		}

		public override object InitializeLifetimeService() {
			return null;
		}

		public void SignalJobProgress(ClientOutputReceiverService.MapResultEventArgs args) {
			if (args.Result.Count != NumberOfSplits)
				return;
			Result = args.Result;
			waitHandle.Set();
		}

		public List<string[]> WaitCompletion() {
			List<string[]> res;
			WaitCompletion(out res);
			return res;
		}

		public void WaitCompletion(out List<string[]> result) {
			if (!isStarted)
				Start();
			var corSvc = RemotingHelper.GetRemoteObject<ClientOutputReceiverService>(ClientService.ClientOutputServiceUri);
			corSvc.SubscribeMapResultComplete(ServiceUri);
			waitHandle.WaitOne();
			result = Result;
		}

		private void Start() {
			isStarted = true;
			RemotingHelper.CreateService(this, ServiceUri, true);
		}
	}
}