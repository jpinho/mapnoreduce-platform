using System.Threading;
using SharedTypes;

namespace PlatformCore
{
	public class TaskRunner : JobTracker
	{
		public TaskRunner(Worker worker)
			: base(worker) {
		}

		public override void Run() {
			base.Run();
			while (Enabled) {
                Thread.Sleep(100);
				TrackJobs();
				MainResetEvent.WaitOne();
			}
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