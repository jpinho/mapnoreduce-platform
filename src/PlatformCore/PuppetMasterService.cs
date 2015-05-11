using SharedTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace PlatformCore {
    [Serializable]
    public class PuppetMasterService : MarshalByRefObject, IPuppetMasterService {
        private readonly object globalLock = new object();
        private readonly object workersLock = new object();
        private readonly object jobTrackersQueueLock = new object();
        private readonly Dictionary<int, IWorker> workers = new Dictionary<int, IWorker>();
        private readonly Dictionary<int, IWorker> workersInUse = new Dictionary<int, IWorker>();
        private readonly Dictionary<int, IWorker> jobTrackersMaster = new Dictionary<int, IWorker>();
        private readonly Queue<Tuple<int, IWorker>> jobTrackersWaitingQueue = new Queue<Tuple<int, IWorker>>();
        public static readonly string ServiceName = "PM";
        public static readonly Uri ServiceUrl = Globals.LocalPuppetMasterUri;

        public PuppetMasterService() {
            // Required for .NET Remoting Proxy Classes.
        }

        public Uri GetServiceUri() {
            return ServiceUrl;
        }

        public void CreateWorker(int workerId, string serviceUrl, string entryUrl) {
            lock (globalLock) {
                var serviceUri = new Uri(serviceUrl);
                RemotingHelper.RegisterChannel(serviceUri);

                var remoteWorker = Worker.Run(workerId, serviceUri, workers, GetServiceUri());
                workers.Add(remoteWorker.WorkerId, remoteWorker);
                remoteWorker.UpdateAvailableWorkers(workers);

                Trace.WriteLine(string.Format("New worker created: id '{0}', url '{1}'."
                    , workerId, serviceUri));

                if (!string.IsNullOrWhiteSpace(entryUrl))
                    NotifyWorkerCreation(remoteWorker.ServiceUrl, entryUrl);
            }
        }

        public Dictionary<int, IWorker> GetWorkersShare(IWorker jobTracker) {
            EnsureRegistedWoker(jobTracker);
            int fairShare = FairScheduler();
            Dictionary<int, IWorker> share = new Dictionary<int, IWorker>();
            if (GetWorkers().Count >= fairShare) {
                share = FairShareExecutor(fairShare);
            } else {
                lock (jobTrackersQueueLock) {
                    GetJobTrackersWaitingQueue().Enqueue(new Tuple<int, IWorker>(fairShare, jobTracker));
                }
            }
            return share;
        }

        private Dictionary<int, IWorker> FairShareExecutor(int fairShare) {
            var filledShare = new Dictionary<int, IWorker>();
            lock (workersLock) {
                for (int i = 0; i < fairShare; i++) {
                    var worker = GetWorkers()[0];
                    filledShare.Add(worker.WorkerId, worker);
                    GetWorkersInUse().Add(worker.WorkerId, worker);
                    GetWorkers().Remove(worker.WorkerId);
                }
            }
            return filledShare;
        }

        private void ProcessPendingShares() {
            lock (jobTrackersQueueLock) {
                while (true) {
                    Tuple<int, IWorker> workerTuple = GetJobTrackersWaitingQueue().Peek();
                    lock (workersLock) {
                        if (!(GetWorkers().Count > workerTuple.Item1))
                            return;
                        GetJobTrackersWaitingQueue().Dequeue();
                        workerTuple.Item2.ReceiveShare(FairShareExecutor(workerTuple.Item1));
                    }
                }
            }
        }

        private void EnsureRegistedWoker(IWorker jobTracker) {
            if (GetJobTrackersMaster().ContainsKey(jobTracker.WorkerId))
                return;
            GetJobTrackersMaster().Add(jobTracker.WorkerId, jobTracker);
        }

        public void ReleaseWorkers(Dictionary<int, IWorker> workersUsed) {
            lock (workersLock) {
                foreach (KeyValuePair<int, IWorker> worker in workersUsed) {
                    GetWorkersInUse().Remove(worker.Key);
                    GetWorkers().Add(worker.Key, worker.Value);
                }
            }
            ProcessPendingShares();
        }

        public int FairScheduler() {
            lock (workersLock) {
                return GetWorkers().Count / GetJobTrackersMaster().Count;
            }
        }

        public void GetStatus() {
            Trace.WriteLine("PuppetMaster [ID: " + ServiceName + "] - Running: '" + ServiceUrl + "'.");
            foreach (var worker in workers.Values) {
                var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
                remoteWorker.GetStatus();
            }
        }

        public Queue<Tuple<int, IWorker>> GetJobTrackersWaitingQueue() {
            return jobTrackersWaitingQueue;
        }

        public Dictionary<int, IWorker> GetWorkers() {
            return workers;
        }

        public Dictionary<int, IWorker> GetJobTrackersMaster() {
            return jobTrackersMaster;
        }

        public Dictionary<int, IWorker> GetWorkersInUse() {
            return workersInUse;
        }

        public void Wait(int seconds) {
            Thread.Sleep(seconds * 1000);
        }

        public void SlowWorker(int workerId, int seconds) {
            IWorker worker;

            try {
                worker = workers[workerId];
            } catch (Exception e) {
                throw new InvalidWorkerIdException(workerId, e);
            }

            var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
            remoteWorker.Slow(seconds);
        }

        public void FreezeWorker(int workerId) {
            IWorker worker;

            try {
                worker = workers[workerId];
            } catch (Exception e) {
                throw new InvalidWorkerIdException(workerId, e);
            }

            var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
            remoteWorker.Freeze();
        }

        public void UnfreezeWorker(int workerId) {
            IWorker worker;

            try {
                worker = workers[workerId];
            } catch (Exception e) {
                throw new InvalidWorkerIdException(workerId, e);
            }

            var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
            remoteWorker.UnFreeze();
        }

        public void FreezeCommunication(int workerId) {
            IWorker worker;

            try {
                worker = workers[workerId];
            } catch (Exception e) {
                throw new InvalidWorkerIdException(workerId, e);
            }

            var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
            remoteWorker.FreezeCommunication();
        }

        public void UnfreezeCommunication(int workerId) {
            IWorker worker;

            try {
                worker = workers[workerId];
            } catch (Exception e) {
                throw new InvalidWorkerIdException(workerId, e);
            }

            var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
            remoteWorker.UnfreezeCommunication();
        }

        private void NotifyWorkerCreation(Uri workerServiceUri, String entryUrl) {
            Trace.WriteLine("Sends notification to worker at ENTRY_URL informing worker creation.");

            var masterWorker = RemotingHelper.GetRemoteObject<IWorker>(entryUrl);
            masterWorker.NotifyWorkerJoin(workerServiceUri);
        }

        /// <summary>
        /// Serves a Marshalled Puppet Master object at a specific IChannel under ChannelServices.
        /// </summary>
        public static void Run() {
            RemotingHelper.RegisterChannel(ServiceUrl);
            RemotingHelper.CreateWellKnownService(typeof(PuppetMasterService), ServiceName);
            Trace.WriteLine("Puppet Master Service endpoint ready at '" + ServiceUrl + "'");
        }
    }
}