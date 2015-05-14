using PlatformCore.Exception;
using SharedTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PlatformCore {
    [Serializable]
    public class PuppetMasterService : MarshalByRefObject, IPuppetMasterService {
        private readonly object globalLock = new object();
        private readonly object workersLock = new object();
        private readonly object workerShareLock = new object();
        private readonly object jobTrackersQueueLock = new object();
        private readonly Dictionary<int, IWorker> workersAvailable = new Dictionary<int, IWorker>();
        private readonly Dictionary<int, IWorker> workersInUse = new Dictionary<int, IWorker>();
        private readonly List<Uri> jobTrackersMaster = new List<Uri>();
        private readonly List<Uri> knownPms = new List<Uri>();
        private readonly Queue<Tuple<int, Uri>> jobTrackersWaitingQueue = new Queue<Tuple<int, Uri>>();

        public static readonly string ServiceName = "PM";
        public static readonly Uri ServiceUrl = Globals.LocalPuppetMasterUri;

        public PuppetMasterService() {
            // Required for .NET Remoting Proxy Classes.
        }

        public Uri GetServiceUri() {
            return ServiceUrl;
        }

        public List<Uri> KnownPmsUris {
            get {
                return knownPms;
            }
        }

        public Dictionary<int, IWorker> WorkersRegistry {
            get {
                return workersAvailable.Union(workersInUse).ToDictionary(p => p.Key, p => p.Value);
            }
        }

        public void AnnouncePm(Uri puppetMasterUrl) {
            if ((KnownPmsUris.Contains(puppetMasterUrl)))
                return;
            KnownPmsUris.Add(puppetMasterUrl);
            Trace.WriteLine("New Puppet Master announced:" + puppetMasterUrl);
            BroadCastPmsList(puppetMasterUrl);
            BroadCastPmsList(ServiceUrl);
        }

        private void BroadCastPmsList(Uri newPm) {
            if (!(KnownPmsUris.Count > 0))
                return;

            var knownPmsDiffNew = from uri in KnownPmsUris
                                  where uri != newPm
                                  select RemotingHelper.GetRemoteObject<IPuppetMasterService>(uri);

            foreach (var pMaster in knownPmsDiffNew) {
                pMaster.AnnouncePm(newPm);
            }
        }

        public void CreateWorker(int workerId, string serviceUrl, string entryUrl) {
            lock (globalLock) {
                var serviceUri = new Uri(serviceUrl);
                RemotingHelper.RegisterChannel(serviceUri);

                var remoteWorker = Worker.Run(workerId, serviceUri, GetServiceUri());
                workersAvailable.Add(remoteWorker.WorkerId, remoteWorker);
                remoteWorker.UpdateAvailableWorkers(workersAvailable);

                Trace.WriteLine(string.Format("New worker created: id '{0}', url '{1}'."
                    , workerId, serviceUri));

                if (!string.IsNullOrWhiteSpace(entryUrl))
                    NotifyWorkerCreation(remoteWorker.ServiceUrl, entryUrl);
            }
        }

        public List<Uri> GetWorkersSharePm(Uri pmUri) {
            var share = new List<Uri>();
            lock (workerShareLock) {
                AnnouncePm(pmUri);
                Trace.WriteLine("Get workers request from PuppetMaster : " + pmUri);
                var fairShare = FairScheduler();
                if (GetAvailableWorkers().Count >= FairScheduler()) {
                    share = FairShareExecutor(fairShare);
                }
            }
            return share;
        }

        public List<Uri> GetWorkersShare(Uri taskRunnerUri) {

            List<Uri> share = new List<Uri>();
            lock (workerShareLock) {
                var tr = RemotingHelper.GetRemoteObject<TaskRunner>(taskRunnerUri);
                if (tr == null)
                    return share;
                EnsureRegistedTaskRunner(taskRunnerUri);
                int fairShare = FairScheduler();
                Trace.WriteLine("Get workers request from taskTracker : " + taskRunnerUri);
                if (GetAvailableWorkers().Count >= fairShare) {
                    share = FairShareExecutor(fairShare);
                } else {
                    share = GetRemoteWorkers(fairShare);
                    Trace.WriteLine("No workers available put JBTM in queue:{0}" + taskRunnerUri);
                    if (!(share.Count > 0)) {
                        lock (jobTrackersQueueLock) {
                            GetJobTrackersWaitingQueue().Enqueue(new Tuple<int, Uri>(fairShare, taskRunnerUri));
                        }
                    }

                }
            }
            return share;
        }

        private List<Uri> GetRemoteWorkers(int workersNeeded) {
            List<Uri> remoteShare = new List<Uri>();
            lock (workersLock) {
                remoteShare = remoteShare.Concat(FairShareExecutor(GetAvailableWorkers().Count())).ToList();
                foreach (Uri pmUri in knownPms) {
                    var pMaster = (IPuppetMasterService)Activator.GetObject(
                        typeof(IPuppetMasterService),
                        pmUri.ToString());
                    var workers = pMaster.GetWorkersSharePm(PuppetMasterService.ServiceUrl);
                    if (workers == null)
                        continue;
                    remoteShare = remoteShare.Concat(workers).ToList();
                    if (remoteShare.Count == workersNeeded)
                        break;
                }
                if (!(remoteShare.Count > workersNeeded)) {
                    ReleaseWorkersOnPms(remoteShare);
                    return new List<Uri>();
                }
            }
            return remoteShare;
        }

        private void ReleaseWorkersOnPms(List<Uri> remoteWorkers) {
            lock (workersLock) {
                foreach (var uri in remoteWorkers) {
                    var wrk = RemotingHelper.GetRemoteObject<IWorker>(uri);
                    var pmr = RemotingHelper.GetRemoteObject<IPuppetMasterService>(wrk.PuppetMasterUri);
                    pmr.ReleaseWorkers(new List<int> { wrk.WorkerId });
                }
            }
        }

        private void EnsureRegistedTaskRunner(Uri trUri) {
            if (GetJobTrackersMaster().Contains(trUri))
                return;
            GetJobTrackersMaster().Add(trUri);
        }

        private List<Uri> FairShareExecutor(int fairShare) {
            var filledShare = new List<Uri>();
            lock (workersLock) {
                for (var i = 0; i < fairShare; i++) {
                    var worker = GetAvailableWorkers().Take(1).First().Value;
                    GetAvailableWorkers().Remove(worker.WorkerId);
                    filledShare.Add(worker.ServiceUrl);
                    GetWorkersInUse().Add(worker.WorkerId, worker);
                    GetAvailableWorkers().Remove(worker.WorkerId);
                }
            }
            return filledShare;
        }

        private void ProcessPendingShares() {
            lock (jobTrackersQueueLock) {
                if (!(GetJobTrackersWaitingQueue().Count > 0))
                    return;
                while (true) {
                    var workerTuple = GetJobTrackersWaitingQueue().Peek();
                    lock (workersLock) {
                        if (!(GetAvailableWorkers().Count > workerTuple.Item1))
                            return;
                        GetJobTrackersWaitingQueue().Dequeue();
                        var tr = RemotingHelper.GetRemoteObject<TaskRunner>(workerTuple.Item2);
                        if (tr == null) {
                            GetJobTrackersWaitingQueue().Enqueue(workerTuple);
                            continue;
                        }
                        tr.ReceiveShare(FairShareExecutor(workerTuple.Item1));
                    }
                }
            }
        }

        public void ReleaseWorkers(List<int> workersUsed) {
            lock (workersLock) {
                foreach (var workerKey in workersUsed) {
                    var worker = GetWorkersInUse()[workerKey];
                    GetWorkersInUse().Remove(workerKey);
                    GetAvailableWorkers().Add(workerKey, worker);
                }
            }
            ProcessPendingShares();
        }

        public int FairScheduler() {
            lock (workersLock) {
                return Convert.ToInt32(Math.Ceiling((double)(GetAvailableWorkers().Count / (GetJobTrackersMaster().Count + KnownPmsUris.Count() * 1.0))));
            }
        }

        public void GetStatus() {
            Trace.WriteLine("PuppetMaster [ID: " + ServiceName + "] - Running: '" + ServiceUrl + "'.");
            foreach (var worker in workersAvailable.Values) {
                var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
                remoteWorker.GetStatus();
            }
        }

        public Queue<Tuple<int, Uri>> GetJobTrackersWaitingQueue() {
            return jobTrackersWaitingQueue;
        }

        public Dictionary<int, IWorker> GetAvailableWorkers() {
            return workersAvailable;
        }

        public List<Uri> GetJobTrackersMaster() {
            return jobTrackersMaster;
        }

        public Dictionary<int, IWorker> GetWorkersInUse() {
            return workersInUse;
        }

        public void SlowWorker(int workerId, int seconds) {
            IWorker worker;

            try {
                worker = WorkersRegistry[workerId];
            } catch (System.Exception e) {
                throw new InvalidWorkerIdException(workerId, e.Message);
            }

            var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
            remoteWorker.Slow(seconds);
        }

        public void FreezeWorker(int workerId) {
            IWorker worker;

            try {
                worker = WorkersRegistry[workerId];
            } catch (System.Exception e) {
                throw new InvalidWorkerIdException(workerId, e.Message);
            }

            var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
            remoteWorker.Freeze();
        }

        public void UnfreezeWorker(int workerId) {
            IWorker worker;

            try {
                worker = WorkersRegistry[workerId];
            } catch (System.Exception e) {
                throw new InvalidWorkerIdException(workerId, e.Message);
            }

            var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
            remoteWorker.UnFreeze();
        }

        public void FreezeCommunication(int workerId) {
            IWorker worker;

            try {
                worker = WorkersRegistry[workerId];
            } catch (System.Exception e) {
                throw new InvalidWorkerIdException(workerId, e.Message);
            }

            var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
            remoteWorker.FreezeCommunication();
        }

        public void UnfreezeCommunication(int workerId) {
            IWorker worker;

            try {
                worker = WorkersRegistry[workerId];
            } catch (System.Exception e) {
                throw new InvalidWorkerIdException(workerId, e.Message);
            }

            var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
            remoteWorker.UnfreezeCommunication();
        }

        private static void NotifyWorkerCreation(Uri workerServiceUri, String entryUrl) {
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