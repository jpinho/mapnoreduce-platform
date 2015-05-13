using System;
using System.Diagnostics;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
	[Serializable]
	public class TaskTracker : JobTracker
	{
		private const int PING_DELAY = 5 * 1000;

		public TaskTracker(Worker worker)
			: base(worker) {
		}

		public override void Run() {
			base.Run();
			while (Enabled) {
				Thread.Sleep(100);
				TrackJob();

				lock (TrackerMutex) {
					if (JobsQueue.Count == 0)
						return;
				}
			}
		}

		private void TrackJob() {
			IJobTask currJob = null;

			// Updates slave job tracker shared state.
			lock (TrackerMutex) {
				if (JobsQueue.Count > 0) {
					Status = JobTrackerState.Busy;
					currJob = CurrentJob = JobsQueue.Dequeue();
				} else {
					Status = JobTrackerState.Available;
					return;
				}
			}

			while (Worker.GetStatus() == WorkerStatus.Busy) {
				try {
					Thread.Sleep(PING_DELAY);
					var masterTracker = RemotingHelper.GetRemoteObject<IJobTracker>(currJob.JobTrackerUri);
					masterTracker.Alive(Worker.WorkerId);
				} catch {
					Trace.WriteLine("TaskTracker failed to send alive signal to TaskRunner. TaskRunner is offline.");
				}
			}
			Status = JobTrackerState.Available;
		}
	}
}