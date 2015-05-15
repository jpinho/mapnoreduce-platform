using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
    [Serializable]
    public class CoordinationManager : IDisposable
    {
        private const int STATUS_UPDATE_TIMEOUT = 10 * 1000;
        private const int REPLICA_RECOVER_ATTEMPT_DELAY = 10 * 1000;
        public const int DEFAULT_NUMBER_OF_REPLICAS = 3;
        private const double REPLICATION_FACTOR = 1;

        private readonly Timer statusUpdatesTimer;
        private volatile JobTracker tracker;
        private List<IWorker> replicas;
        private readonly List<ISlaveReplica> replicasObjects = new List<ISlaveReplica>();
        private readonly Dictionary< /*worker url*/ Uri, /*lastping*/ DateTime> replicasAliveSignals;
        private readonly object rmanagerMutex = new object();
        private bool isStarted;

        public int NumberOfReplicas { get; set; }

        public CoordinationManager(JobTracker tracker) {
            NumberOfReplicas = DEFAULT_NUMBER_OF_REPLICAS;
            this.tracker = tracker;
            replicasAliveSignals = new Dictionary<Uri, DateTime>();
            statusUpdatesTimer = new Timer(StatusUpdate, 0, Timeout.Infinite, STATUS_UPDATE_TIMEOUT);
        }

        private List<IWorker> PickReplicas() {
            Trace.WriteLine("CoordinatorManager picking replicas for fault tolerance.");
            var repsCount = GetWiseNumberForReplicas(tracker.Worker.GetWorkersList().Count);

            var reps = (
                from wk in tracker.Worker.GetWorkersList().Take(repsCount)
                where wk.ServiceUrl != tracker.Worker.ServiceUrl
                select wk
            ).ToList();

            Trace.WriteLine("CoordinatorManager just picked " + repsCount + " replicas from PuppetMaster.");
            return reps;
        }

        /// <summary>
        /// Calculates how many replicas would be nice to have based on the logarithm of number of workers.
        /// </summary>
        /// <param name="workersCount">number of workers</param>
        private static int GetWiseNumberForReplicas(int workersCount) {
            if (workersCount < 1)
                return 0;
            return Math.Min(
                workersCount, Convert.ToInt32(Math.Round(Math.Ceiling(Math.Log(workersCount, 2)) * REPLICATION_FACTOR, 0)));
        }

        public void Start() {
            if (isStarted)
                return;
            isStarted = true;

            // grab replicas to backup master tracker
            replicas = PickReplicas();
            if (replicas.Count < 1) {
                Trace.WriteLine("CoordinationManager not replicating anything since there are not enough workers to support replication.");
                return;
            }

            var i = 1;
            replicas.ForEach(wk => {
                lock (rmanagerMutex) {
                    replicasAliveSignals[wk.ServiceUrl] = DateTime.Now;
                }

                // start replica trackers on target workers
                var replicaTracker = RemotingHelper.GetRemoteObject<IWorker>(wk.ServiceUrl)
                    .StartReplicaTracker(i++);

                Trace.WriteLine("Creating replica at worker '" + replicaTracker.Worker.WorkerId
                    + "' with priority '" + replicaTracker.Priority + "'.");

                // save replica objects (SlaveReplica)
                replicasObjects.Add(replicaTracker);
            });

            // starts the update timer
            statusUpdatesTimer.Change(0, STATUS_UPDATE_TIMEOUT);
        }

        public void ReplicaAliveSignal(Uri workerUrl) {
            lock (rmanagerMutex) {
                replicasAliveSignals[workerUrl] = DateTime.Now;
                Trace.WriteLine("CoordinationManager received ping signal from replica/worker URL:'" + workerUrl
                    + "' at '" + DateTime.Now.ToString("ddMMyyyyTHH:mm:ss:fff") + "'.");
            }
        }

        public void PauseStateUpdates() {
            statusUpdatesTimer.Change(Timeout.Infinite, STATUS_UPDATE_TIMEOUT);
        }

        public void ResumeStateUpdates() {
            statusUpdatesTimer.Change(0, STATUS_UPDATE_TIMEOUT);
        }

        private void StatusUpdate(object state) {
            var replicaRecoveryThreads = new List<Thread>();

            foreach (var replica in replicas) {
                try {
                    Trace.WriteLine("CoordinationManager sending JobTracker state to ReplicaWorker ID:"
                        + replica.WorkerId + ".");
                    var remoteReplica = RemotingHelper.GetRemoteObject<IWorker>(replica.ServiceUrl);
                    remoteReplica.UpdateReplicas(replicasObjects);
                    remoteReplica.ReceiveJobTrackerState(tracker.GetState());
                } catch {
                    Trace.WriteLine("CoordinationManager received an error contacting replica.");
                    TimeSpan lastReplicaUpdate;

                    lock (rmanagerMutex) {
                        lastReplicaUpdate = DateTime.Now.Subtract(replicasAliveSignals[replica.ServiceUrl]);
                    }

                    // if false we need to recover that lost replica if possible
                    if (!(lastReplicaUpdate.TotalSeconds > SlaveReplica.PING_DELAY * 3))
                        continue;

                    Trace.WriteLine("CoordinatorManager detected that replica/worker ID:"
                        + replica.WorkerId + " seems to be permanently crashed.");

                    // enqueue job to be done regarding, recovering lost replicas
                    replicaRecoveryThreads.Add(new Thread(() => {
                        replicas.RemoveAll(r => r.ServiceUrl.Equals(replica.ServiceUrl));
                        replicasObjects.RemoveAll(r => r.Worker.ServiceUrl.Equals(replica.ServiceUrl));

                        var attempts = 0;
                        while (!RecoverCrashedReplica() && attempts++ <= 3) {
                            Trace.WriteLine("Replica crashed and not recovered, waiting...");
                            Thread.Sleep(REPLICA_RECOVER_ATTEMPT_DELAY);
                        }
                    }));
                }
            }

            // starts recovery of replicas
            replicaRecoveryThreads.ForEach(thr => thr.Start());
        }

        private bool RecoverCrashedReplica() {
            if (tracker.Worker.GetWorkersList().Count <= 1)
                return false;

            Trace.WriteLine("Recovering crashed replica.");
            var availableForReplication = tracker.Worker.GetWorkersList();

            availableForReplication.RemoveAll(worker => {
                return
                    replicas.Exists(
                        wk => wk.ServiceUrl == worker.ServiceUrl);
            });

            availableForReplication.RemoveAll(worker => worker.ServiceUrl == tracker.Worker.ServiceUrl);

            if (availableForReplication.Count == 0) {
                Trace.WriteLine("RecoverCrashedReplica failed to acquire a new replica.");
                return false;
            }

            var reps = (from wk in availableForReplication.Take(1) select wk).ToList();
            Trace.WriteLine("Crashed replica recovered.");

            var replica = reps.First();
            replicas.Add(replica);

            lock (rmanagerMutex) {
                replicasAliveSignals[replica.ServiceUrl] = DateTime.Now;
            }

            var replicaTracker = RemotingHelper.GetRemoteObject<IWorker>(replica.ServiceUrl)
                .EnsureReplicaTracker();
            RemotingHelper.GetRemoteObject<IWorker>(replica.ServiceUrl)
                .ReceiveJobTrackerState(tracker.GetState());
            replicasObjects.Add(replicaTracker);
            return true;
        }

        public void Dispose() {
            Trace.WriteLine("CoordinationManager disposing.");
            statusUpdatesTimer.Dispose();
            replicas.ForEach(worker => {
                RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl)
                    .DestroyReplica();
            });
        }
    }
}