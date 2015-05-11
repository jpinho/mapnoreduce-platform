using System;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
	[Serializable]
	public class TaskTracker : JobTracker
	{
		public TaskTracker(Worker worker)
			: base(worker) {
		}

		public override void Run() {
			base.Run();
			while (Enabled) {
				Thread.Sleep(100);
				TrackJob();
			}
		}

		private void TrackJob() {
			IJobTask currJob = null;

			// Updates slave job tracker shared state.
			lock (TrackerMutex) {
				if (JobsQueue.Count > 0) {
					Status = JobTrackerState.Busy;
					currJob = CurrentJob = JobsQueue.Dequeue();
				} else
					Status = JobTrackerState.Available;
			}

			if (currJob == null)
				return;

			while (Worker.GetStatus() == WorkerStatus.Busy) {
				var masterTracker = RemotingHelper.GetRemoteObject<IJobTracker>(currJob.JobTrackerUri);
				masterTracker.Alive(Worker.WorkerId);
			}
		}
	}
}