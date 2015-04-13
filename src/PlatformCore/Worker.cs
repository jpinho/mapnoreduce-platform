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
		private readonly object workerJobLock = new object();
		private readonly object workerReceiveJobLock = new object();
		private readonly AutoResetEvent freezeHandle = new AutoResetEvent(false);

		/// <summary>
		/// The job activeTracker used to coordinate a job or to report progress about it's own job
		/// execution. Whether this job activeTracker performes in one way or the other depends on
		/// its mode property <see cref="JobTrackerStatus"/>.
		/// </summary>
		private readonly JobTracker activeTracker;

		private readonly Thread thrActiveTracker;

		private readonly JobTracker passiveTracker;

		private readonly Thread thrPassiveTracker;

		public delegate bool ExecuteMapJobDelegate(JobTask task);

		/// <summary>
		/// This worker id.
		/// </summary>
		public int WorkerId { get; set; }

		/// <summary>
		/// List of all workers known by this worker.
		/// </summary>
		public Dictionary<int /*worker id*/, IWorker> WorkersList { get; private set; }

		/// <summary>
		/// The service URL used to reach this work remotely.
		/// </summary>
		public Uri ServiceUrl { get; set; }

		public WorkerStatus Status { get; set; }

		public Worker() {
			Status = WorkerStatus.Available;
			activeTracker = new JobTracker(this, JobTrackerMode.Active);
			passiveTracker = new JobTracker(this, JobTrackerMode.Passive);

			thrActiveTracker = new Thread(() => {
				activeTracker.Start();
			});

			thrPassiveTracker = new Thread(() => {
				passiveTracker.Start();
			});
		}

		public Worker(int workerId, Uri serviceUrl, Dictionary<int, IWorker> availableWorkers)
			: this() {
			WorkerId = workerId;
			ServiceUrl = serviceUrl;
			WorkersList = availableWorkers;
		}

		/// <summary>
		/// Overrides the default object leasing behavior such that the object is kept in memory as
		/// long as the host application domain is running.
		/// </summary>
		/// <see><cref>https://msdn.microsoft.com/en-us/library/ms973841.aspx</cref></see>
		public override Object InitializeLifetimeService() {
			return null;
		}

		public void UpdateAvailableWorkers(Dictionary<int, IWorker> availableWorkers) {
			WorkersList = availableWorkers;
		}

		public WorkerStatus GetStatus() {
			Trace.WriteLine("Worker [ID: " + WorkerId + "] - Status: '" + Status.ToString() + "'.");
			return Status;
		}

		/// <summary>
		/// Receives a Map job and distributes the processing of their contents accross multiple
		/// workers. Each worker processes a split of the given filePath (used as file ID), calling
		/// the Map method of the received class name of the assembly code.
		/// </summary>
		/// <param name="task">The job to be processed.</param>
		public void ReceiveMapJob(IJobTask task) {
			lock (workerReceiveJobLock) {
				Trace.WriteLine("New map job received by worker [ID: " + WorkerId + "].\n"
					+ "Master Job Tracker Uri: '" + activeTracker.ServiceUri + "'");
				task.JobTrackerUri = activeTracker.ServiceUri;
				activeTracker.ScheduleJob(task);
			}
		}

		public bool ExecuteMapJob(IJobTask task) {
			lock (workerJobLock) {
				passiveTracker.ScheduleJob(task);

				try {
					var splitProvider = (IClientSplitProviderService)Activator.GetObject(
						typeof(IClientSplitProviderService),
						task.SplitProviderUrl);

					var data = splitProvider.GetFileSplit(task.FileName, task.SplitNumber);
					var assembly = Assembly.Load(task.MapFunctionAssembly);

					Thread.Sleep(/*work delay simulation*/ 10 * 1000);

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
		}

		public void AsyncExecuteMapJob(IJobTracker jobTracker, int split, IWorker remoteWorker, AsyncCallback callback, IJobTask job) {
			var fnExecuteMapJob = new ExecuteMapJobDelegate(remoteWorker.ExecuteMapJob);
			var newTask = (JobTask)job.Clone();
			newTask.SplitNumber = split;

			Trace.WriteLine("Begin invoke job on worker [ID: " + WorkerId + "].");
			fnExecuteMapJob.BeginInvoke(newTask, callback, null);
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
			activeTracker.FreezeCommunication();
			freezeHandle.WaitOne();
		}

		public void UnFreeze() {
			freezeHandle.Set();
			activeTracker.UnfreezeCommunication();
		}

		public void FreezeCommunication() {
			activeTracker.FreezeCommunication();
		}

		public void UnfreezeCommunication() {
			activeTracker.UnfreezeCommunication();
		}
	}
}