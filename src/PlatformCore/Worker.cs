using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
    public class Worker : MarshalByRefObject, IWorker
    {

        private readonly AutoResetEvent freezeHandle = new AutoResetEvent(false);

        /// <summary>
        /// The job trackers used to coordinate a job or to report progress about it's own job
        /// execution. Whether this job tracker performes in one way or the other depends on its
        /// mode property <see cref="JobTracker.JobTrackerStatus"/>.
        /// </summary>
        private readonly List<JobTracker> trackers = new List<JobTracker>();

        /// <summary>
        /// List of all workers known by this worker, that are online.
        /// </summary>
        private Dictionary<int /*worker id*/, IWorker> onlineWorkers = new Dictionary<int, IWorker>();

        /// <summary>
        /// List of workers that are processing Job Tasks.
        /// </summary>
        private readonly Dictionary<int /*worker id*/, JobTask> busyWorkers = new Dictionary<int, JobTask>();

        /// <summary>
        /// List of workers that are not responding anymore.
        /// </summary>
        private readonly List<IWorker> offlineWorkers = new List<IWorker>();

        public delegate bool ExecuteMapJobDelegate(JobTask task);

        /// <summary>
        /// This worker id.
        /// </summary>
        public int WorkerId { get; set; }

        public Dictionary<int /*worker id*/, IWorker> OnlineWorkers {
            get { return onlineWorkers; }
        }

        public List<IWorker> OfflineWorkers {
            get { return offlineWorkers; }
        }

        public Dictionary<int /*worker id*/, JobTask> BusyWorkers {
            get { return busyWorkers; }
        }

        /// <summary>
        /// The service URL used to reach this work remotely.
        /// </summary>
        public Uri ServiceUrl { get; set; }

        public Worker() {
            // Required for .NET Remoting Proxy Classes.
        }

        public Worker(int workerId, Uri serviceUrl, Dictionary<int, IWorker> availableWorkers) {
            WorkerId = workerId;
            ServiceUrl = serviceUrl;
            this.onlineWorkers = availableWorkers;
        }

        public void UpdateAvailableWorkers(Dictionary<int, IWorker> availableWorkers) {
            this.onlineWorkers = availableWorkers;
        }

        public void GetStatus() {
            Debug.WriteLine("I'm fine, thanks.");
        }

        /// <summary>
        /// Receives a Map job and distributes the processing of their contents accross multiple
        /// workers. Each worker processes a split of the given filePath (used as file ID), calling
        /// the Map method of the received class name of the assembly code in <paramref name="mapAssemblyCode"/>.
        /// </summary>
        /// <param name="job">The job to be processed.</param>
        public void ReceiveMapJob(IJobTask job) {
            // Starts the Job Tracker thread, to execute apart from the worker thread.
            var tracker = new JobTracker(this, job);
            trackers.Add(tracker);

            new Thread(new ThreadStart(delegate {
                tracker.Start(JobTracker.JobTrackerStatus.ACTIVE);
            })).Start();
        }

        public void AsyncExecuteMapJob(IJobTracker jobTracker, int split, IWorker remoteWorker, AsyncCallback callback, IJobTask job) {
            var fnExecuteMapJob = new Worker.ExecuteMapJobDelegate(remoteWorker.ExecuteMapJob);
            var newTask = (JobTask)job.Clone();
            newTask.SplitNumber = split;
            fnExecuteMapJob.BeginInvoke(newTask, callback, null);

            Trace.WriteLine(string.Format("Job split {0} sent to worker at '{1}'."
                , newTask.SplitNumber, remoteWorker.ServiceUrl));

            Trace.WriteLine("Split " + newTask.SplitNumber + " removed from splits queue.");
        }

        public bool ExecuteMapJob(IJobTask task) {
            var passiveTracker = new JobTracker(this, task);
            var thrPassiveTracker = new Thread(new ThreadStart(delegate {
                passiveTracker.Start(
                    JobTracker.JobTrackerStatus.PASSIVE);
            }));
            thrPassiveTracker.Start();

            try {
                var splitProvider = (IClientSplitProviderService)Activator.GetObject(
                    typeof(IClientSplitProviderService),
                    task.SplitProviderUrl);

                var data = splitProvider.GetFileSplit(task.FileName, task.SplitNumber);
                var assembly = Assembly.Load(task.MapFunctionAssembly);

                foreach (var type in assembly.GetTypes()) {
                    if (!type.IsClass || !type.FullName.EndsWith("." + task.MapClassName))
                        continue;

                    var mapperClassObj = Activator.CreateInstance(type);
                    object[] args = { data };

                    var result = (IList<KeyValuePair<string, string>>)type.InvokeMember("Map",
                        BindingFlags.Default | BindingFlags.InvokeMethod, null, mapperClassObj, args);

                    Trace.WriteLine("Map call result was: ");
                    foreach (var p in result)
                        Trace.WriteLine("key: " + p.Key + ", value: " + p.Value);

                    var outputReceiver = (IClientOutputReceiverService)Activator.GetObject(
                        typeof(IClientOutputReceiverService),
                        task.OutputReceiverUrl);

                    outputReceiver.ReceiveMapOutputFragment(
                        task.FileName
                        , (from r in result select r.Key + " " + r.Value).ToArray()
                        , task.SplitNumber);

                    return true;
                }
                return false;
            } finally {
                thrPassiveTracker.Abort();
            }
        }

        internal Dictionary<int, IWorker> GetActiveWorkers() {
            //TODO: Implement this
            return null;
        }

        internal static Worker Run(int workerId, Uri serviceUrl, Dictionary<int, IWorker> workers) {
            var wrk = new Worker(workerId, serviceUrl, workers);
            RemotingHelper.CreateService(wrk, serviceUrl);
            return wrk;
        }

        public void Slow(int secs) {
            Thread.Sleep(secs * 1000);
        }

        public void Freeze() {
            foreach (var tracker in trackers)
                tracker.FreezeCommunication();
            freezeHandle.WaitOne();
        }

        public void UnFreeze() {
            freezeHandle.Set();
            foreach (var tracker in trackers)
                tracker.UnfreezeCommunication();
        }

        public void FreezeCommunication() {
            foreach (var tracker in trackers)
                tracker.FreezeCommunication();
        }

        public void UnfreezeCommunication() {
            foreach (var tracker in trackers)
                tracker.UnfreezeCommunication();
        }
    }
}