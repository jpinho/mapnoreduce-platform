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
	[Serializable]
	public class Worker : MarshalByRefObject, IWorker
	{
		public const int NOTIFY_TIMEOUT = 1000 * 5;
		private readonly object workerLock = new object();
		private readonly object workerReceiveJobLock = new object();
		private readonly AutoResetEvent freezeHandle = new AutoResetEvent(false);
		public bool PassiveInitialized = false;
		/// <summary>
		/// The job activeTracker used to coordinate a job or to report progress about it's own job
		/// execution. Whether this job activeTracker performes in one way or the other depends on
		/// its mode property <see cref="JobTrackerState"/>.
		/// </summary>
		private JobTracker activeTracker;
		private Thread thrActiveTracker;
		private JobTracker passiveTracker;
		private Thread thrPassiveTracker;
		private bool trackersInitialized;
		// possible worker states
		private enum State { Running, Failed, Frozen };

		// initial state
		private List<ManualResetEvent> frozenRequests = new List<ManualResetEvent>();
		/// <summary>
		/// List of all workers known by this worker.
		/// </summary>
		private Dictionary<int /*worker id*/, IWorker> workersList;

		public delegate bool ExecuteMapJobDelegate(JobTask task);

		/// <summary>
		/// This worker id.
		/// </summary>
		public int WorkerId { get; set; }

		/// <summary>
		/// The service URL used to reach this work remotely.
		/// </summary>
		public Uri ServiceUrl { get; set; }

		public WorkerStatus Status { get; set; }

		public Worker() {
			Status = WorkerStatus.Available;
		}

		public Worker(int workerId, Uri serviceUrl, Dictionary<int, IWorker> availableWorkers)
			: this() {
			WorkerId = workerId;
			ServiceUrl = serviceUrl;
			workersList = availableWorkers;
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
			StateCheck();
			lock (workerLock) {
				workersList = availableWorkers;
			}
		}

		//wakes requests frozen during frozen state

		private bool ProcessFrozenRequests() {
			for (int i = 0; i < frozenRequests.Count; i++) {
				ManualResetEvent mre = frozenRequests[i];
				mre.Set();
			}
			return true;
		}

		//puts to sleep all incoming requests while worker is frozen

		private void StateCheck() {
			WorkerStatus status;
			lock (workerLock) {
				status = Status;
			}

			if (status == WorkerStatus.Offline)
				throw new RemotingException("Server is Offline;");
			else if (status == WorkerStatus.Frozen) {
				var mre = new ManualResetEvent(false);
				frozenRequests.Add(mre);
				mre.WaitOne();
			}
		}

		public Dictionary<int, IWorker> GetWorkersList() {
			lock (workerLock) {
				return workersList;
			}
		}

		public WorkerStatus GetStatus() {
			lock (workerLock) {
				//Trace.WriteLine("Worker [ID: " + WorkerId + "] - Status: '" + Status.ToString() + "'.");
				return Status;
			}
		}

		public void SetStatus(WorkerStatus status) {
			lock (workerLock) {
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
				if (!trackersInitialized)
					InitTrackers();

				Trace.WriteLine("New map job received by worker [ID: " + WorkerId + "].\n"
					+ "Master Job Tracker Uri: '" + activeTracker.ServiceUri + "'");
				task.JobTrackerUri = activeTracker.ServiceUri;
				activeTracker.ScheduleJob(task);
				WakeTrackers();

			}
		}

		private void WakeTrackers() {
			this.activeTracker.Wake();
		}

		private void InitTrackers() {
			trackersInitialized = true;
			InitPassiveTracker();
			activeTracker = new JobTracker(this, JobTrackerMode.Active);

			thrActiveTracker = new Thread(() => {
				activeTracker.Start();
			});

			thrActiveTracker.Start();
		}

		private void InitPassiveTracker() {
			if (PassiveInitialized)
				return;
			PassiveInitialized = true;
			passiveTracker = new JobTracker(this, JobTrackerMode.Passive);

			thrPassiveTracker = new Thread(() => {
				passiveTracker.Start();
			});
			thrPassiveTracker.Start();
		}

		public bool ExecuteMapJob(IJobTask task) {
			StateCheck();
			lock (workerLock) {
				if (!PassiveInitialized)
					InitPassiveTracker();
				else {
					Trace.WriteLine("Trackers already initialized");
				}
				passiveTracker.ScheduleJob(task);
				Status = WorkerStatus.Busy;
			}

			try {
				var splitProvider = (IClientSplitProviderService)Activator.GetObject(
					typeof(IClientSplitProviderService),
					task.SplitProviderUrl);

				var data = splitProvider.GetFileSplit(task.FileName, task.SplitNumber);
				var assembly = Assembly.Load(task.MapFunctionAssembly);

				//Thread.Sleep( /*work delay simulation*/ 30 * 1000);

				foreach (var type in assembly.GetTypes()) {
					if (!type.IsClass || !type.FullName.EndsWith("." + task.MapClassName, StringComparison.InvariantCultureIgnoreCase))
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
				Trace.WriteLine("Send alives thread finished on worker [ID: '" + WorkerId + "'].");

				lock (workerLock) {
					Status = WorkerStatus.Available;
				}
			}
		}

		public void AsyncExecuteMapJob(int split,
				string fileName, List<int> fileSplits, Uri jobTrackerUri, string mapClassName,
				byte[] mapFunctionName, string outputReceiverUrl, string splitProviderUrl) {
			StateCheck();

			//var fnExecuteMapJob = new ExecuteMapJobDelegate(ExecuteMapJob);
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

			// The callback called after the execution of the async method call.
			//var callback = new AsyncCallback(result => {
			//});

			Trace.WriteLine("Begin invoke job on worker [ID: " + WorkerId + "].");
			//fnExecuteMapJob.BeginInvoke(newTask, callback, null);
			ExecuteMapJob(newTask);
		}

		internal static Worker Run(int workerId, Uri serviceUrl, Dictionary<int, IWorker> workers) {
			var wrk = new Worker(workerId, serviceUrl, workers);
			RemotingHelper.CreateService(wrk, serviceUrl);
			return wrk;
		}

		public void Slow(int secs) {
			StateCheck();
			Thread.Sleep(secs * 1000);
		}

		public void Freeze() {
			WorkerStatus status;
			lock (workerLock) {
				status = Status;
			}
			if (status != WorkerStatus.Frozen) {
				lock (workerLock) {
					Status = WorkerStatus.Frozen;
				}
				frozenRequests.Clear();
				//activeTracker.FreezeCommunication();
			}
		}

		public void UnFreeze() {
			WorkerStatus status;
			lock (workerLock) {
				status = Status;
			}
			if (status == WorkerStatus.Offline || status == WorkerStatus.Frozen) {
				lock (workerLock) {
					Status = WorkerStatus.Available;
				}
				bool frozenWakeResult = ProcessFrozenRequests();
			}
			//activeTracker.UnfreezeCommunication();
		}

		public void FreezeCommunication() {
			StateCheck();
			activeTracker.FreezeCommunication();
		}

		public void UnfreezeCommunication() {
			StateCheck();
			activeTracker.UnfreezeCommunication();
		}

	}
}