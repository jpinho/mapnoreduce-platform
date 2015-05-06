using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
	[Serializable]
	public class JobTracker : MarshalByRefObject, IJobTracker
	{
		private const int PROCESSING_DELAY_TIMEOUT = 2000;
        public static AutoResetEvent mainResetEvent = new AutoResetEvent(false);
		private readonly object trackerMutex = new object();
		private readonly Worker worker;
		private readonly Queue<IJobTask> jobQueue = new Queue<IJobTask>();
		private volatile Dictionary</*splitnumber*/ int, /*workerid*/ int> splitsBeingProcessed = new Dictionary<int, int>();
		private volatile Dictionary</*workerid*/ int, /*lastupdate*/DateTime> workerAliveSignals = new Dictionary<int, DateTime>();
		/// <summary>
		/// The job splits priority queue.
		/// </summary>
		private Queue<int> splitsQueue;

        // initial state
        private List<ManualResetEvent> frozenRequests = new List<ManualResetEvent>();

		public IJobTask CurrentJob { get; private set; }
		public Uri ServiceUri { get; set; }
		public JobTrackerMode Mode { get; set; }
		public JobTrackerState Status { get; set; }

		public JobTracker() {
			Mode = JobTrackerMode.Passive;
			Status = JobTrackerState.Available;
		}

		public JobTracker(Worker worker, JobTrackerMode trackerMode)
			: this() {
			this.worker = worker;
			Mode = trackerMode;

			ServiceUri = Mode == JobTrackerMode.Active ?
				new Uri("tcp://localhost:" + worker.ServiceUrl.Port + "/MasterJobTracker-W" + worker.WorkerId) :
				new Uri("tcp://localhost:" + worker.ServiceUrl.Port + "/SlaveJobTracker-W" + worker.WorkerId);
		}

		[SuppressMessage("ReSharper", "FunctionNeverReturns")]
		public void Start() {
            stateCheck();
			switch (Mode) {
				case JobTrackerMode.Active:
					if (ServiceUri == null)
						ServiceUri = new Uri("tcp://localhost:" + worker.ServiceUrl.Port + "/MasterJobTracker-W" + worker.WorkerId);
					RemotingServices.Marshal(this, "MasterJobTracker-W" + worker.WorkerId, typeof(IJobTracker));
                    new Thread(delegate()
                    {
                        while (true)
                        {
                            Thread.Sleep(100);
                            MasterTrackerMain();
                            mainResetEvent.WaitOne();
                        }
                    }).Start();
                    break;
				case JobTrackerMode.Passive:
					if (ServiceUri == null)
						ServiceUri = new Uri("tcp://localhost:" + worker.ServiceUrl.Port + "/SlaveJobTracker-W" + worker.WorkerId);
					RemotingServices.Marshal(this, "SlaveJobTracker-W" + worker.WorkerId, typeof(IJobTracker));
                    new Thread(delegate()
                    {
                        while (true)
                        {
                            Thread.Sleep(100);
                            SlaveTrackerMain();
                        }
                    }).Start();
                    break;
			}
		}

		private void MasterTrackerMain() {
            //Trace.WriteLine("Tracker Running --------" + worker.WorkerId);
            stateCheck();
			lock (trackerMutex) {
				// If job tracker busy or without jobs to process exit.
                if (!(jobQueue.Count > 0) || (!(jobQueue.Count > 0) && Status != JobTrackerState.Available))
                {
                    Status = JobTrackerState.Available;
					return;
                }
				// Get next job and set state to busy.
				CurrentJob = jobQueue.Dequeue();
				splitsQueue = new Queue<int>(CurrentJob.FileSplits);
				Status = JobTrackerState.Busy;
			}

			var thrMonitor = new Thread(MonitorSplitProcessing);
			thrMonitor.Start();

			// Loops until all splits are processed, i.e, job done.
			while (splitsQueue.Count > 0 || splitsBeingProcessed.Count != 0) {
				// Selects from all online workers, those that are not busy.
				var availableWorkers = new Queue<IWorker>((
						from w in worker.GetWorkersList()
						where w.Value.GetStatus() == WorkerStatus.Available
						select w.Value
					).ToList());

				SplitsDelivery(availableWorkers, CurrentJob);
				Thread.Sleep(PROCESSING_DELAY_TIMEOUT);
			}

			lock (trackerMutex) {
				CurrentJob = null;
				splitsBeingProcessed.Clear();
				splitsQueue.Clear();
			}

			thrMonitor.Join();
		}

		private void SlaveTrackerMain() {
            stateCheck();
			IJobTask currentJob = null;

			// Updates slave job tracker shared state.
			lock (trackerMutex) {
				if (jobQueue.Count > 0) {
					currentJob = CurrentJob = jobQueue.Dequeue();
					Status = JobTrackerState.Busy;
				} else
					Status = JobTrackerState.Available;
			}

			if (currentJob == null)
				return;

			while (worker.GetStatus() == WorkerStatus.Busy) {
				var masterTracker = RemotingHelper.GetRemoteObject<IJobTracker>(currentJob.JobTrackerUri);
				masterTracker.Alive(worker.WorkerId);
				Thread.Sleep(Worker.NOTIFY_TIMEOUT);
			}
		}

		private void SplitsDelivery(Queue<IWorker> availableWorkers, IJobTask job) {
            stateCheck();
			int splitsCount;

			lock (trackerMutex) {
				splitsCount = splitsQueue.Count;
				if (splitsCount == 0) {
					return;
				}
			}

			// Delivers as many splits as it cans, considering the number of available workers.
			for (var i = 0; i < Math.Min(availableWorkers.Count, splitsCount); i++) {
				IWorker remoteWorker;
				int split;

				lock (trackerMutex) {
					var wrk = availableWorkers.Dequeue();
					remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(wrk.ServiceUrl);
					split = splitsQueue.Peek();
                    
				}

				try {
					// Async call to ExecuteMapJob.
					Trace.WriteLine(string.Format("Job split {0} sent to worker at '{1}'.", split, remoteWorker.ServiceUrl));
					remoteWorker.SetStatus(WorkerStatus.Busy);

					new Thread(() => {
						remoteWorker.AsyncExecuteMapJob(split, job.FileName, job.FileSplits, job.JobTrackerUri,
							job.MapClassName, job.MapFunctionAssembly, job.OutputReceiverUrl, job.SplitProviderUrl);

						lock (trackerMutex) {
							splitsBeingProcessed.Remove(split);
						}

						remoteWorker.SetStatus(WorkerStatus.Available);
						Trace.WriteLine(string.Format("Worker '{0}' finished processing split number '{1}'."
							, remoteWorker.ServiceUrl, split));
					}).Start();

					lock (trackerMutex) {
						splitsQueue.Dequeue();
						Trace.WriteLine("Split " + split + " removed from splits queue.");

						workerAliveSignals[remoteWorker.WorkerId] = DateTime.Now;
						splitsBeingProcessed.Add(split, remoteWorker.WorkerId);
					}
				} catch (RemotingException ex) {
					Trace.WriteLine(ex.GetType().FullName + " - " + ex.Message
						+ " -->> " + ex.StackTrace);
				}
			}
		}

		private void MonitorSplitProcessing() {
            stateCheck();
			while (true) {
				lock (trackerMutex) {
					if (CurrentJob == null)
						return;

					var splitsInProcess = splitsBeingProcessed.ToArray();
					foreach (var keyValue in splitsInProcess) {
						var workerId = keyValue.Value;
						var split = keyValue.Key;
						TimeSpan tspan;

						lock (trackerMutex) {
							tspan = DateTime.Now.Subtract(workerAliveSignals[workerId]);
						}

						if (!(tspan.TotalSeconds > 120.0))
							continue;

						// Worker not responding.
						worker.Status = WorkerStatus.Offline;
						splitsQueue.Enqueue(split);
						splitsBeingProcessed.Remove(split);
					}
				}

				Thread.Sleep(Worker.NOTIFY_TIMEOUT);
			}
		}

		public void Alive(int wid) {
            stateCheck();
			Trace.WriteLine("Alive signal worker '" + wid + "'.");
			lock (trackerMutex) {
				var w = ((Worker)worker.GetWorkersList()[wid]);
				if (w.Status == WorkerStatus.Offline)
					w.SetStatus(WorkerStatus.Available);
				workerAliveSignals[wid] = DateTime.Now;
			}
		}

		public void FreezeCommunication() {
            JobTrackerState state;
            lock (trackerMutex)
            {
                state = Status;
            }
            if (state != JobTrackerState.Frozen)
            {
                lock (trackerMutex)
                {
                    Status = JobTrackerState.Frozen;
                }
                frozenRequests.Clear();
            }
		}

		public void UnfreezeCommunication() {
            JobTrackerState state;
            lock (trackerMutex)
            {
                state = Status;
            }
            if (state == JobTrackerState.Frozen)
            {
                lock (trackerMutex)
                {
                    Status = JobTrackerState.Available;
                }
                bool frozenWakeResult = processFrozenRequests();
            }
		}

        //wakes requests frozen during frozen state
        private bool processFrozenRequests()
        {
            for (int i = 0; i < frozenRequests.Count; i++)
            {
                ManualResetEvent mre = frozenRequests[i];
                mre.Set();
            }
            return true;
        }

        //puts to sleep all incoming requests while worker is frozen
        private void stateCheck()
        {
            JobTrackerState state;
            lock (trackerMutex)
            {
                state = Status;
            }

            if (state == JobTrackerState.Frozen)
            {
                var mre = new ManualResetEvent(false);
                frozenRequests.Add(mre);
                mre.WaitOne();
            }
        }

		public void ScheduleJob(IJobTask job) {
            stateCheck();
			lock (trackerMutex) {
				jobQueue.Enqueue(job);
			}
		}

        internal void Wake()
        {
            mainResetEvent.Set();
        }
    }
}