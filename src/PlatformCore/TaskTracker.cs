using SharedTypes;
using System;
using System.Threading;

namespace PlatformCore {
    [Serializable]
    public class TaskTracker : JobTracker {
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
                }
            }

            if (currJob == null)
                return;

            while (Worker.GetStatus() == WorkerStatus.Busy) {
                var masterTracker = RemotingHelper.GetRemoteObject<IJobTracker>(currJob.JobTrackerUri);
                masterTracker.Alive(Worker.WorkerId);
            }
            Status = JobTrackerState.Available;
        }
    }
}