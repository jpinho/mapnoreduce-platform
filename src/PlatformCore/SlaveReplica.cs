using System;
using System.Diagnostics;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
	[Serializable]
	public class SlaveReplica : IDisposable
	{
		private readonly Timer heartbeat;
		private bool isInitialized = false;
		public readonly Worker Worker;
		public const int PING_DELAY = 5000;
		public Tuple<JobTrackerStateInfo, DateTime> MasterJobTrackerState { get; private set; }
		public bool Enabled { get; set; }

		public SlaveReplica(Worker worker) {
			Worker = worker;
			heartbeat = new Timer(SendHearbeat, null, Timeout.Infinite, PING_DELAY);
		}

		private void Init() {
			isInitialized = true;
			heartbeat.Change(0, PING_DELAY);
		}

		private void SendHearbeat(object state) {
			var masterTracker = RemotingHelper.GetRemoteObject<TaskRunner>(MasterJobTrackerState.Item1.ServiceUri);
			masterTracker.AliveReplica(Worker.WorkerId);
			Trace.WriteLine("JobTrackerSlave: Sending Ping to JobTrackerMaster - WorkerID:" + Worker.WorkerId + ".");
		}

		public void SaveState(JobTrackerStateInfo state) {
			if (!isInitialized)
				Init();
			MasterJobTrackerState = new Tuple<JobTrackerStateInfo, DateTime>(state, DateTime.Now);
			Trace.WriteLine("JobTrackerSlave: SaveSate at '" + DateTime.Now.ToString("dd-MM-yyyyTHH:mm:ss:fff") + "'.");
		}

		public void Dispose() {
			heartbeat.Dispose();
			MasterJobTrackerState = null;
		}
	}
}