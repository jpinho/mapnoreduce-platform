using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading;
using PlatformCore.Properties;
using SharedTypes;

namespace PlatformCore
{
    [Serializable]
    public class Worker : MarshalByRefObject, IWorker
    {
        public const int NOTIFY_TIMEOUT = 1000 * 5;

        private readonly object workerMutex = new object();
        private readonly object workerReceiveJobLock = new object();
        private JobTracker taskTracker;
        private JobTracker masterTracker;
        private readonly List<ManualResetEvent> frozenRequests = new List<ManualResetEvent>();

        private SlaveReplica replicaTracker;
        public enum State { Running, Failed, Frozen };
        public delegate bool ExecuteMapJobDelegate(JobTask task);

        /// <summary>
        /// This worker id.
        /// </summary>
        public int WorkerId { get; set; }
        /// <summary>
        /// The service URL used to reach this work remotely.
        /// </summary>
        public Uri ServiceUrl { get; set; }
        public Uri PuppetMasterUri { get; set; }
        public WorkerStatus Status { get; set; }

        /// <summary>
        /// List of all workers known by this worker.
        /// </summary>
        public Dictionary<int, IWorker> WorkersList { get; set; }

        public Worker() {
            Status = WorkerStatus.Available;
        }

        public Worker(int workerId, Uri serviceUrl, Uri puppetMasterServiceUri)
            : this() {
            WorkerId = workerId;
            ServiceUrl = serviceUrl;
            WorkersList = new Dictionary<int, IWorker>();
            PuppetMasterUri = puppetMasterServiceUri;
        }

        /// <summary>
        /// Overrides the default object leasing behavior such that the object is kept in memory as
        /// long as the host application domain is running.
        /// </summary>
        /// <see><cref>https://msdn.microsoft.com/en-us/library/ms973841.aspx</cref></see>
        public override object InitializeLifetimeService() {
            return null;
        }

        public void UpdateAvailableWorkers(Dictionary<int, IWorker> availableWorkers) {
            StateCheck();
            lock (workerMutex) {
                WorkersList = new Dictionary<int, IWorker>(availableWorkers);
            }
        }

        public Dictionary<int, IWorker> GetIWorkerObjects(List<Uri> workersList) {
            return workersList
                .Select(RemotingHelper.GetRemoteObject<IWorker>)
                .Where(wrk => wrk != null)
                .ToDictionary(wrk => wrk.WorkerId);
        }

        public void NotifyWorkerJoin(Uri serviceUri) {
            StateCheck();
            var workerToAdd = RemotingHelper.GetRemoteObject<IWorker>(serviceUri);
            lock (workerMutex) {
                try {
                    WorkersList.Add(workerToAdd.WorkerId, workerToAdd);

                } catch (System.Exception e) {
                    Trace.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// Wakes requests frozen during frozen state.
        /// </summary>
        /// <returns></returns>
        private void ProcessFrozenRequests() {
            foreach (var mre in frozenRequests) {
                mre.Set();
            }
        }

        /// <summary>
        /// Puts to sleep all incoming requests while worker is frozen.
        /// </summary>
        private void StateCheck() {
            WorkerStatus status;
            lock (workerMutex) {
                status = Status;
            }

            switch (status) {
                case WorkerStatus.Offline:
                    throw new RemotingException("Server is Offline;");
                case WorkerStatus.Frozen:
                    var mre = new ManualResetEvent(false);
                    frozenRequests.Add(mre);
                    mre.WaitOne();
                    break;
            }
        }

        private void EnsureMasterTracker() {
            if (masterTracker != null)
                return;
            masterTracker = new TaskRunner(this);
            new Thread(TaskRunner_MainLoop).Start();
        }

        private void EnsureMasterTracker(JobTrackerStateInfo crashedTrackerState) {
            masterTracker = new TaskRunner(this, crashedTrackerState);
            new Thread(TaskRunner_MainLoop).Start();
        }

        private void EnsureTaskTracker() {
            if (taskTracker != null)
                return;
            taskTracker = new TaskTracker(this);
            new Thread(TaskTracker_MainLoop).Start();
        }

        private void TaskRunner_MainLoop() {
            masterTracker.Run();

            lock (workerReceiveJobLock) {
                masterTracker.Dispose();
                masterTracker = null;
            }
        }

        private void TaskTracker_MainLoop() {
            taskTracker.Run();

            lock (workerReceiveJobLock) {
                taskTracker.Dispose();
                taskTracker = null;
            }
        }

        public Dictionary<int, IWorker> GetWorkersList() {
            lock (workerMutex) {
                return WorkersList;
            }
        }

        public WorkerStatus GetStatus() {
            lock (workerMutex) {
                Trace.WriteLine("Worker [ID: " + WorkerId + "] - Status: '" + Status + "'.");
                return Status;
            }
        }

        public void SetStatus(WorkerStatus status) {
            lock (workerMutex) {
                Status = status;
            }
        }

        /// <summary>
        /// Receives a Map job and distributes the processing of their contents accross multiple
        /// workers. Each worker processes a split of the given filePath (used as file ID), calling
        /// the Map method of the received class name of the assembly code.
        /// </summary>
        /// <param name="task">The job to be processed.</param>
        public void ReceiveMapJob(IJobTask task) {
            StateCheck();
            lock (workerReceiveJobLock) {
                EnsureMasterTracker();
                Trace.WriteLine("New map job received by worker [ID: " + WorkerId + "].\n"
                    + "Master Job Tracker Uri: '" + masterTracker.ServiceUri + "'");

                task.JobTrackerUri = masterTracker.ServiceUri;
                masterTracker.ScheduleJob(task);
                masterTracker.Wake();
            }
        }

        public void ExecuteMapJob(int split,
                string fileName, List<int> fileSplits, Uri jobTrackerUri, string mapClassName,
                byte[] mapFunctionName, string outputReceiverUrl, string splitProviderUrl) {

            var newTask = new JobTask {
                FileName = fileName,
                SplitNumber = split,
                FileSplits = fileSplits,
                JobTrackerUri = jobTrackerUri,
                MapClassName = mapClassName,
                MapFunctionAssembly = mapFunctionName,
                OutputReceiverUrl = outputReceiverUrl,
                SplitProviderUrl = splitProviderUrl
            };

            Trace.WriteLine("Executing job on worker [ID: " + WorkerId + "].");
            ExecuteMapJob(newTask);
        }

        public void ReceiveJobTrackerState(JobTrackerStateInfo state) {
            EnsureReplicaTracker().SaveState(state);
            LogTrackerState(state);
        }

        private static void LogTrackerState(JobTrackerStateInfo state) {
            var filename = string.Empty;
            var jobsCount = 0;
            var workersAliveCount = 0;

            if (state.CurrentJob != null)
                filename = state.CurrentJob.FileName;
            if (state.JobsQueue != null)
                jobsCount = state.JobsQueue.Count;
            if (state.WorkerAliveSignals != null)
                workersAliveCount = state.WorkerAliveSignals.Count;

            Trace.WriteLine(string.Format(Resources.JobTrackerStateSummaryString
                , filename
                , state.Enabled
                , jobsCount
                , state.ServiceUri
                , state.Status
                , state.WorkerId
                , workersAliveCount));
        }

        public void DestroyReplica() {
            if (replicaTracker == null)
                return;
            replicaTracker.Dispose();
            replicaTracker = null;
            Trace.WriteLine("Worker/Replica '" + WorkerId + "' received replica destroy signal. Replica Destroyed!");
        }

        public void SendReplicaState(ISlaveReplica slaveReplica) {
            if (replicaTracker == null)
                return;
            replicaTracker.ReceiveRecoveryState(slaveReplica);
            Trace.WriteLine("Worker/Replica '" + WorkerId + "' received replica '" + slaveReplica.Worker.WorkerId + "' state.");
        }

        public void PromoteToMaster(JobTrackerStateInfo masterJobTrackerState) {
            lock (workerReceiveJobLock) {
                EnsureMasterTracker(masterJobTrackerState);
            }
            DestroyReplica();
        }

        public void UpdateReplicas(List<ISlaveReplica> replicasGroup) {
            if (replicaTracker == null)
                return;
            replicaTracker.UpdateReplicas(replicasGroup);
        }

        public ISlaveReplica StartReplicaTracker(int priority) {
            EnsureReplicaTracker().Priority = priority;
            return replicaTracker;
        }

        public ISlaveReplica EnsureReplicaTracker() {
            if (replicaTracker != null)
                return replicaTracker;
            replicaTracker = new SlaveReplica(this);
            Trace.WriteLine("Worker/Replica started SlaveTracker to handle state updates from Master Tracker.");
            return replicaTracker;
        }

        public void ExecuteMapJob(IJobTask task) {
            StateCheck();
            lock (workerMutex) {
                EnsureTaskTracker();
                taskTracker.ScheduleJob(task);
                Status = WorkerStatus.Busy;
            }

            try {
                var splitProvider = (IClientSplitProviderService)Activator.GetObject(
                    typeof(IClientSplitProviderService),
                    task.SplitProviderUrl);

                var data = splitProvider.GetFileSplit(task.FileName, task.SplitNumber);
                var assembly = Assembly.Load(task.MapFunctionAssembly);

                foreach (var type in assembly.GetTypes()) {
                    if (!type.IsClass || !type.FullName.EndsWith("." + task.MapClassName, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    var mapperClassObj = Activator.CreateInstance(type);
                    object[] args = { data };

                    var result = (IList<KeyValuePair<string, string>>)type.InvokeMember("Map",
                        BindingFlags.Default | BindingFlags.InvokeMethod, null, mapperClassObj, args);

                    Trace.WriteLine("MAPPER: Map call executed with success!");

                    var outputReceiver = (IClientOutputReceiverService)Activator.GetObject(
                        typeof(IClientOutputReceiverService),
                        task.OutputReceiverUrl);

                    outputReceiver.ReceiveMapOutputFragment(
                        task.FileName
                        , (from r in result select r.Key + " " + r.Value).ToArray()
                        , task.SplitNumber);

                    return;
                }
            } finally {
                Trace.WriteLine("Send alives thread finished on worker [ID: '" + WorkerId + "'].");
                lock (workerMutex) {
                    Status = WorkerStatus.Available;
                }
            }
        }

        internal static Worker Run(int workerId, Uri serviceUrl, Uri puppetMasterServiceUri) {
            var wrk = new Worker(workerId, serviceUrl, puppetMasterServiceUri);
            RemotingHelper.CreateService(wrk, serviceUrl);
            return wrk;
        }

        public void ReleaseWorkers() {
            try {
                var ppm = RemotingHelper.GetRemoteObject<PuppetMasterService>(PuppetMasterService.ServiceUrl);
                ppm.ReleaseWorkers(GetWorkersList().Keys.ToList());
            } catch (System.Exception e) {
                Trace.WriteLine(e.Message);

            }
            UpdateAvailableWorkers(new Dictionary<int, IWorker>());
        }

        public void Slow(int secs) {
            StateCheck();
            Thread.Sleep(secs * 1000);
        }

        public void Freeze() {
            WorkerStatus status;
            lock (workerMutex) {
                status = Status;
            }
            if (status == WorkerStatus.Frozen)
                return;

            lock (workerMutex) {
                Status = WorkerStatus.Frozen;
            }
            frozenRequests.Clear();
        }

        public void UnFreeze() {
            WorkerStatus status;
            lock (workerMutex) {
                status = Status;
            }

            if (status != WorkerStatus.Offline && status != WorkerStatus.Frozen)
                return;
            lock (workerMutex) {
                Status = WorkerStatus.Available;
            }
            ProcessFrozenRequests();
        }

        public void FreezeCommunication() {
            lock (workerMutex) {
                if (masterTracker != null) {
                    Trace.WriteLine("MasterTracker ## FREEZED");
                    masterTracker.FreezeCommunication();
                } else
                    Trace.WriteLine("FreezeCommunication uneffective master tracker is NULL.");
            }
        }

        public void UnfreezeCommunication() {
            lock (workerMutex) {
                if (masterTracker != null) {
                    Trace.WriteLine("MasterTracker ## UN-FREEZED");
                    masterTracker.UnfreezeCommunication();
                } else
                    Trace.WriteLine("UnfreezeCommunication uneffective master tracker is NULL.");
            }
        }
    }
}