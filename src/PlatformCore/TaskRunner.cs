using SharedTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace PlatformCore {
    [Serializable]
    public class TaskRunner : JobTracker {
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
                PullAvailableWorkers();
                Status = JobTrackerState.Busy;
            }

            var thrScheduler = new Thread(() => (new DefaultJobScheduler(this, CurrentJob)).Run());
            thrScheduler.Start();
            thrScheduler.Join();
            Worker.ReleaseWorkers();
            lock (TrackerMutex) {
                CurrentJob = null;
                Status = JobTrackerState.Available;
            }
        }

        public void ReceiveShare(Dictionary<int /*worker id*/, IWorker> share) {
            Worker.UpdateAvailableWorkers(share);
            WaitForShareEvent.Set();
        }

        public void PullAvailableWorkers() {
            var pMaster = (IPuppetMasterService)Activator.GetObject(
                typeof(IPuppetMasterService),
                PuppetMasterService.ServiceUrl.ToString());
            try {
                Worker.UpdateAvailableWorkers(pMaster.GetWorkersShare(this.ServiceUri));
            } catch (Exception e) {
                Trace.WriteLine(e.Message);
            } finally {
                if (!(Worker.GetWorkersList().Count > 0)) {
                    Worker.SetStatus(WorkerStatus.Busy);
                    WaitForShareEvent.WaitOne();
                }
            }
        }

    }
}