using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
    public class JobTracker : MarshalByRefObject, IJobTracker
    {
        public IJobTask Task { get; set; }
        private AutoResetEvent freezeHandle = new AutoResetEvent(false);

        private Worker worker;

        /// <summary>
        /// The job splits priority queue.
        /// </summary>
        private Queue<int> splitsQueue;

        public enum JobTrackerStatus { ACTIVE, PASSIVE };
        private JobTrackerStatus mode = JobTrackerStatus.PASSIVE;
        private DateTime lastHeartBeat = DateTime.UtcNow.Date;

        public JobTracker() {
            // Required for .NET Remoting Proxy Classes.
        }

        public JobTracker(Worker worker, IJobTask task) {
            Task = task;
            this.worker = worker;
        }

        public void Start(JobTrackerStatus trackerMode) {
            // Register Services (alive, complete) Do Work
            if (Enum.Equals(trackerMode, JobTrackerStatus.ACTIVE)) {
                RemotingServices.Marshal(this, "JobTracker", typeof(IJobTracker));

                // Converts splits to priority queue.
                splitsQueue = new Queue<int>(Task.FileSplits);

                while (splitsQueue.Count > 0) {
                    // Selects from all online workers those that are not busy.
                    var availableWorkers = new Queue<IWorker>((
                            from onlineWorker in worker.OnlineWorkers
                            where !worker.BusyWorkers.ContainsKey(onlineWorker.Key /*worker id*/)
                            select onlineWorker.Value
                        ).ToList());

                    SplitsDelivery(availableWorkers, Task);
                    Thread.Sleep(2000);
                }
            }
        }

        private void SplitsDelivery(Queue<IWorker> availableWorkers, IJobTask job) {
            // Delivers as many splits as it cans, considering the number of available workers.
            for (var i = 0; i < Math.Min(availableWorkers.Count, splitsQueue.Count); i++) {
                var remoteWorker = availableWorkers.Dequeue();
                var split = splitsQueue.Peek();

                try {
                    // The callback called after the execution of the async method call.
                    var callback = new AsyncCallback((result) => {
                        Trace.WriteLine(string.Format("Worker '{0}' finished processing split number '{1}'."
                            , remoteWorker.ServiceUrl, split));
                    });

                    // Async call to ExecuteMapJob.
                    remoteWorker.AsyncExecuteMapJob(this, split, remoteWorker, callback, job);
                    splitsQueue.Dequeue();
                } catch (RemotingException ex) {
                    Trace.WriteLine(ex.GetType().FullName + " - " + ex.Message
                        + " -->> " + ex.StackTrace);
                }
            }
        }

        public void Alive(int wid) {
            /*TODO fault tolerant algorithm*/
        }

        public void Complete(int wid) {
            //Remove worker from active list
            worker.OnlineWorkers.Remove(wid);
        }

        public void FreezeCommunication() {
            freezeHandle.WaitOne();
        }

        public void UnfreezeCommunication() {
            freezeHandle.Set();
        }
    }
}