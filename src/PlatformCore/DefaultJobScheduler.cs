using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading;
using System.Threading.Tasks;
using SharedTypes;

namespace PlatformCore
{
	/// <summary>
	/// Default Job Scheduler - Schedules job splits to workers using a FIFO data structure.
	/// </summary>
	[Serializable]
	public class DefaultJobScheduler
	{
		public readonly JobTracker Tracker;
		public readonly IJobTask CurrentJob;

		/// <summary>
		/// The job splits priority queue.
		/// </summary>
		private readonly Queue<int> splitsQueue;
		private volatile Dictionary</*splitnumber*/ int, /*workerid*/ int> splitsBeingProcessed = new Dictionary<int, int>();
		private readonly object schedulerMutex = new object();

		public DefaultJobScheduler(JobTracker tracker, IJobTask currentJob) {
			Tracker = tracker;
			CurrentJob = currentJob;
			splitsQueue = new Queue<int>(currentJob.FileSplits);
		}

		public void Run() {
			Tracker.StateCheck();

			while (true) {
				lock (schedulerMutex) {
					if (CurrentJob == null)
						break;
				}

				if (splitsQueue.Count > 0 || splitsBeingProcessed.Count != 0) {
					// Selects from all online workers, those that are not busy.
					var availableWorkers = new Queue<IWorker>((
							from w in Tracker.Worker.GetWorkersList()
							where w.Value.GetStatus() == WorkerStatus.Available
							select w.Value
						).ToList());
					SplitsDelivery(availableWorkers, CurrentJob);
				} else
					break;

				CheckWorkersState();
				Thread.Sleep(Worker.NOTIFY_TIMEOUT);
			}

			lock (schedulerMutex) {
				splitsBeingProcessed.Clear();
				splitsQueue.Clear();
			}
		}

		private void CheckWorkersState() {
			lock (schedulerMutex) {
				var splitsInProcess = splitsBeingProcessed.ToArray();

				foreach (var keyValue in splitsInProcess) {
					var workerId = keyValue.Value;
					var split = keyValue.Key;
					TimeSpan tspan;

					lock (schedulerMutex) {
						tspan = DateTime.Now.Subtract(Tracker.WorkerAliveSignals[workerId]);
					}

					if (!(tspan.TotalSeconds > 60.0))
						continue;

					// Worker not responding.
					Trace.WriteLine("Worker '" + workerId + "' not responding (split '" + split + "').");
					var worker = Tracker.Worker.GetWorkersList()[workerId];
					if (worker != null) {
						worker.SetStatus(WorkerStatus.Offline);
					}
					splitsQueue.Enqueue(split);
					splitsBeingProcessed.Remove(split);
				}
			}
		}

		private void SplitsDelivery(Queue<IWorker> availableWorkers, IJobTask job) {
			Tracker.StateCheck();

			int splitsCount;
			lock (schedulerMutex) {
				splitsCount = splitsQueue.Count;
				if (splitsCount == 0) {
					return;
				}
			}

			// Delivers as many splits as it cans, considering the number of available workers.
			for (var i = 0; i < Math.Min(availableWorkers.Count, splitsCount); i++) {
				IWorker remoteWorker;
				int split;

				lock (schedulerMutex) {
					var wrk = availableWorkers.Dequeue();
					remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(wrk.ServiceUrl);
					split = splitsQueue.Peek();
				}

				try {
					// Async call to ExecuteMapJob.
					Trace.WriteLine(string.Format("Job split {0} sent to worker at '{1}'.", split, remoteWorker.ServiceUrl));
					remoteWorker.SetStatus(WorkerStatus.Busy);

					var asyncTask = new Task(() => {
						remoteWorker.ExecuteMapJob(split, job.FileName, job.FileSplits, job.JobTrackerUri,
							job.MapClassName, job.MapFunctionAssembly, job.OutputReceiverUrl, job.SplitProviderUrl);
					});

					asyncTask.GetAwaiter().OnCompleted(() => {
						lock (schedulerMutex) {
							splitsBeingProcessed.Remove(split);
						}

						remoteWorker.SetStatus(WorkerStatus.Available);
						Trace.WriteLine(string.Format("Worker '{0}' finished processing split number '{1}'."
							, remoteWorker.ServiceUrl, split));
					});
					asyncTask.Start();

					lock (schedulerMutex) {
						splitsQueue.Dequeue();
						Trace.WriteLine("Split " + split + " removed from splits queue.");

						Tracker.WorkerAliveSignals[remoteWorker.WorkerId] = DateTime.Now;
						splitsBeingProcessed.Add(split, remoteWorker.WorkerId);
					}
				} catch (RemotingException ex) {
					Trace.WriteLine(ex.GetType().FullName + " - " + ex.Message
						+ " -->> " + ex.StackTrace);
				}
			}
		}
	}
}