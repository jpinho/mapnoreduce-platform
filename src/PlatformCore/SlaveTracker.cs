using System;
using System.Diagnostics;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
	[Serializable]
	public class SlaveTracker : JobTracker
	{
		public const int PING_DELAY = 5000;
		public Tuple<JobTrackerStateInfo, DateTime> MasterJobTrackerState { get; private set; }

		public SlaveTracker(Worker worker)
			: base(worker) {
		}

		public override void Run() {
			base.Run();
			while (Enabled) {
				PingMaster();
				Thread.Sleep(PING_DELAY);
			}
		}

		private void PingMaster() {
			var masterTracker = RemotingHelper.GetRemoteObject<TaskRunner>(MasterJobTrackerState.Item1.ServiceUri);
			masterTracker.AliveReplica(Worker.WorkerId);
			Trace.WriteLine("JobTrackerSlave: Sending Ping to JobTrackerMaster - WorkerID:" + Worker.WorkerId + ".");
		}

		public void SaveState(JobTrackerStateInfo state) {
			MasterJobTrackerState = new Tuple<JobTrackerStateInfo, DateTime>(state, DateTime.Now);
			Trace.WriteLine("JobTrackerSlave: SaveSate at '" + DateTime.Now.ToString("dd-MM-yyyyTHH:mm:ss:fff") + "'.");
		}
	}
}