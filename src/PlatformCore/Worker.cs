using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
    public class Worker : MarshalByRefObject, IWorker
    {
        /// <summary>
        /// The job splits priority queue.
        /// </summary>
        private Queue<int> splitsQueue;

        /// <summary>
        /// The job tracker used to coordinate a job or to report progress about it's own job
        /// execution. Whether this job tracker performes in one way or the other depends on its
        /// mode property <see cref="JobTracker.JobTrackerStatus"/>.
        /// </summary>
        private JobTracker tracker;

        /// <summary>
        /// List of all workers known by this worker, that are online.
        /// </summary>
        private Dictionary<int /*worker id*/, IWorker> onlineWorkers = new Dictionary<int, IWorker>();

        /// <summary>
        /// List of workers that are processing Job Tasks.
        /// </summary>
        private Dictionary<int /*worker id*/, JobTask> busyWorkers = new Dictionary<int, JobTask>();

        /// <summary>
        /// List of workers that are not responding anymore.
        /// </summary>
        private List<IWorker> offlineWorkers = new List<IWorker>();

        public delegate bool ExecuteMapJobDelegate(JobTask task);

        /// <summary>
        /// This worker id.
        /// </summary>
        public int WorkerId { get; set; }

        /// <summary>
        /// The service URL used to reach this work remotely.
        /// </summary>
        public Uri ServiceUrl { get; set; }

        public Worker(int workerId, Uri serviceUrl, Dictionary<int, IWorker> availableWorkers) {
            WorkerId = workerId;
            ServiceUrl = serviceUrl;
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
            // Converts splits to priority queue.
            splitsQueue = new Queue<int>(job.FileSplits);

            // Selects from all online workers those that are not busy.
            var availableWorkers = new Queue<IWorker>((
                    from onlineWorker in onlineWorkers
                    where !busyWorkers.ContainsKey(onlineWorker.Key /*worker id*/)
                    select onlineWorker.Value
                ).ToList());

            // Delivers as many splits as it cans, considering the number of available workers.
            for (var i = 0; i < Math.Min(availableWorkers.Count, splitsQueue.Count); i++) {
                var worker = availableWorkers.Dequeue();
                var split = splitsQueue.Peek();

                try {
                    var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(
                        worker.ServiceUrl.OriginalString);

                    if (remoteWorker == null) {
                        offlineWorkers.Add(worker);
                        Trace.WriteLine("Could not locate worker at '" + worker.ServiceUrl + "'.");
                        continue;
                    }

                    // The callback called after the execution of the async method call.
                    var callback = new AsyncCallback((result) => {
                        Trace.WriteLine(string.Format("Worker '{0}' finished processing split number '{1}'."
                            , remoteWorker.ServiceUrl, split));
                    });

                    // Async call to ExecuteMapJob.
                    var fnExecuteMapJob = new Worker.ExecuteMapJobDelegate(remoteWorker.ExecuteMapJob);
                    var newTask = (JobTask)job.Clone();
                    newTask.SplitNumber = split;
                    fnExecuteMapJob.BeginInvoke(newTask, callback, null);
                    Trace.WriteLine(string.Format("Job split {0} sent to worker at '{1}'."
                        , newTask.SplitNumber, worker.ServiceUrl));

                    // Removes the peeked split from the queue.
                    splitsQueue.Dequeue();
                    Trace.WriteLine("Split " + newTask.SplitNumber + " removed from splits queue.");
                } catch (RemotingException ex) {
                    Trace.WriteLine(ex.GetType().FullName + " - " + ex.Message
                        + " -->> " + ex.StackTrace);
                }
            }

            // Starts the Job Tracker thread, to execute apart from the worker thread.
            new Thread(new ThreadStart(delegate {
                tracker = new JobTracker(this);
                tracker.Start(JobTracker.JobTrackerStatus.ACTIVE);
            })).Start();
        }

        public bool ExecuteMapJob(IJobTask task) {
            new Thread(new ThreadStart(delegate {
                tracker = new JobTracker(this);
                tracker.Start(JobTracker.JobTrackerStatus.PASSIVE);
            })).Start();

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

                return true;
            }
            return false;
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
    }
}