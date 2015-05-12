using System;
using System.Diagnostics;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
	[Serializable]
	public class TaskRunner : JobTracker
	{
		private CoordinationManager replicaManager;

		public TaskRunner(Worker worker)
			: base(worker) {
		}

		public override void Run() {
			base.Run();
			Trace.WriteLine("TaskRunner starting CoordinationManager for fault tolerance.");

			using (replicaManager = new CoordinationManager(this)) {
				replicaManager.Start();
				while (Enabled) {
					Thread.Sleep(100);
					TrackJobs();

					lock (TrackerMutex) {
						if (JobsQueue.Count == 0)
							return;
					}
				}
			}
		}

		public void AliveReplica(int workerId) {
			replicaManager.ReplicaAliveSignal(workerId);
		}

		private void TrackJobs() {
			lock (TrackerMutex) {
				if (JobsQueue.Count == 0)
					return;
				// Get next job and set state to busy.
				CurrentJob = JobsQueue.Dequeue();
				Status = JobTrackerState.Busy;
			}

			var thrScheduler = new Thread(() => (new DefaultJobScheduler(this, CurrentJob)).Run());
			thrScheduler.Start();
			thrScheduler.Join();

			lock (TrackerMutex) {
				CurrentJob = null;
				Status = JobTrackerState.Available;
			}
		}
	}
}