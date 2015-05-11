using System;
using System.Diagnostics;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
	[Serializable]
	public class TaskRunner : JobTracker
	{
		private readonly CoordinationManager replicaManager;

		public TaskRunner(Worker worker)
			: base(worker) {
			replicaManager = new CoordinationManager(this);
		}

		public override void Run() {
			base.Run();
			Trace.WriteLine("TaskRunner starting CoordinationManager for fault tolerance.");
			replicaManager.Start();

			while (Enabled) {
				Thread.Sleep(100);
				TrackJobs();
				MainResetEvent.WaitOne();
			}
		}

		public void AliveReplica(int workerId) {
			replicaManager.ReplicaAliveSignal(workerId);
		}

		private void TrackJobs() {
			lock (TrackerMutex) {
				// If job tracker busy or without jobs to process exit.
				if (!(JobsQueue.Count > 0) || (!(JobsQueue.Count > 0) && Status != JobTrackerState.Available)) {
					Status = JobTrackerState.Available;
					return;
				}
				// Get next job and set state to busy.
				CurrentJob = JobsQueue.Dequeue();
				Status = JobTrackerState.Busy;
			}

			var thrScheduler = new Thread(() => (new DefaultJobScheduler(this, CurrentJob)).Run());
			thrScheduler.Start();
			thrScheduler.Join();

			lock (TrackerMutex) {
				CurrentJob = null;
			}
		}
	}
}