using System;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
	[Serializable]
	public class SlaveReplica : IDisposable
	{
		public const int PING_DELAY = 5000;
		private readonly Timer heartbeat;
		private bool isInitialized;
		public readonly Worker Worker;
		public Tuple<JobTrackerStateInfo, DateTime> MasterJobTrackerState { get; private set; }
		public bool Enabled { get; set; }

		public SlaveReplica(Worker worker) {
			Worker = worker;
			heartbeat = new Timer(SendHeartbeat, null, Timeout.Infinite, PING_DELAY);
		}

		private void Init() {
			isInitialized = true;
			heartbeat.Change(0, PING_DELAY);
		}

		private void SendHeartbeat(object state) {
			try {
				Trace.WriteLine("JobTrackerSlave: Sending Ping to JobTrackerMaster - WorkerID:" + Worker.WorkerId + ".");
				var masterTracker = RemotingHelper.GetRemoteObject<TaskRunner>(MasterJobTrackerState.Item1.ServiceUri);
				masterTracker.AliveReplica(Worker.WorkerId);
			} catch (RemotingException) {
				Trace.WriteLine("SlaveReplica heartbeat failed, master tracker seems to be offline!");
			} catch (System.Exception ex) {
				Trace.WriteLine("SlaveReplica heartbeat failed, unknown error: '" + ex.Message + "'.");
			}
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