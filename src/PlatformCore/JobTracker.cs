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
	public class JobTracker : MarshalByRefObject, IJobTracker
	{
		private const int NOTIFY_TIMEOUT = 1000 * 5;
		private const int PROCESSING_DELAY_TIMEOUT = 2000;
		private readonly object trackerMutex = new object();
		private readonly Worker worker;
		private readonly Queue<IJobTask> jobQueue = new Queue<IJobTask>();
		private readonly Dictionary<int, DateTime> workerAliveSignals = new Dictionary<int, DateTime>();
		/// <summary>
		/// The job splits priority queue.
		/// </summary>
		private Queue<int> splitsQueue;

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
		}

		[SuppressMessage("ReSharper", "FunctionNeverReturns")]
		public void Start() {
			switch (Mode) {
				case JobTrackerMode.Active:
					ServiceUri = new Uri("tcp://localhost:" + worker.ServiceUrl.Port + "MasterJobTracker-W" + worker.WorkerId);
					RemotingServices.Marshal(this, "MasterJobTracker-W" + worker.WorkerId, typeof(IJobTracker));
					while (true) {
						MasterTrackerMain();
					}
				case JobTrackerMode.Passive:
					ServiceUri = new Uri("tcp://localhost:" + worker.ServiceUrl.Port + "SlaveJobTracker-W" + worker.WorkerId);
					RemotingServices.Marshal(this, "SlaveJobTracker-W" + worker.WorkerId, typeof(IJobTracker));
					while (true) {
						SlaveTrackerMain();
					}
			}
		}

		private void MasterTrackerMain() {
			IJobTask currentJob = null;

			// Updates master job tracker shared state.
			lock (trackerMutex) {
				if (jobQueue.Count > 0) {
					currentJob = CurrentJob = jobQueue.Dequeue();
					splitsQueue = new Queue<int>(currentJob.FileSplits);
					Status = JobTrackerState.Busy;
				} else
					Status = JobTrackerState.Available;
			}

			if (currentJob == null)
				return;

			// Loops until all splits are processed => Job Done.
			while (splitsQueue.Count > 0) {
				// Selects from all online workers those that are not busy.
				var availableWorkers = new Queue<IWorker>((
						from onlineWorker in worker.OnlineWorkers
						where onlineWorker.Value.GetStatus() == WorkerStatus.Available
						select onlineWorker.Value
					).ToList());

				SplitsDelivery(availableWorkers, currentJob);
				Thread.Sleep(PROCESSING_DELAY_TIMEOUT);
			}
		}

		private void SlaveTrackerMain() {
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
				Thread.Sleep(NOTIFY_TIMEOUT);
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
					Trace.WriteLine(string.Format("Job split {0} sent to worker at '{1}'.", job.SplitNumber, remoteWorker.ServiceUrl));
					remoteWorker.AsyncExecuteMapJob(this, split, remoteWorker, callback, job);

					splitsQueue.Dequeue();
					Trace.WriteLine("Split " + job.SplitNumber + " removed from splits queue.");

					//new Thread(() => MonitorSplitProcessing(job, split)).Start();
				} catch (RemotingException ex) {
					Trace.WriteLine(ex.GetType().FullName + " - " + ex.Message
						+ " -->> " + ex.StackTrace);
				}
			}
		}

		/// <summary>
		/// TODO: I was doing this! Very probably it haves a bug!
		/// </summary>
		/// <param name="job"></param>
		/// <param name="splitNumber"></param>
		private void MonitorSplitProcessing(IJobTask job, int splitNumber) {
			while (true) {
				foreach (var aliveSignal in workerAliveSignals) {
					if (DateTime.Now.Subtract(aliveSignal.Value).Seconds > 30) {
						lock (trackerMutex) {
							splitsQueue.Enqueue(splitNumber);
						}
					}
				}
				Thread.Sleep(NOTIFY_TIMEOUT);
			}
		}

		public void Alive(int wid) {
			lock (trackerMutex) {
				if (Mode != JobTrackerMode.Active)
					return;
				workerAliveSignals[wid] = DateTime.Now;
			}
		}

		public void FreezeCommunication() {
			var dte = new EnvDTE.DTE();
			var thread = dte.Debugger.CurrentThread;
			thread.Freeze();
		}

		public void UnfreezeCommunication() {
			var dte = new EnvDTE.DTE();
			var thread = dte.Debugger.CurrentThread;
			thread.Thaw();
		}

		public void ScheduleJob(IJobTask job) {
			lock (trackerMutex) {
				jobQueue.Enqueue(job);
			}
		}
	}
}