using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
	[Serializable]
	public abstract class JobTracker : MarshalByRefObject, IJobTracker, IDisposable
	{
		protected const string PREF_MASTER_SVC_NAME = "MasterTracker";
		protected const string PREF_TRACKER_SVC_NAME = "TaskTracker";

		private volatile Dictionary</*workerid*/ int, /*lastupdate*/DateTime> workerAliveSignals = new Dictionary<int, DateTime>();
		private readonly List<ManualResetEvent> frozenRequests = new List<ManualResetEvent>();
		private Queue<IJobTask> jobsQueue = new Queue<IJobTask>();
		protected readonly object TrackerMutex = new object();

		public volatile bool Enabled = true;
		public Worker Worker { get; set; }
		public static AutoResetEvent MainResetEvent = new AutoResetEvent(false);
		public Uri ServiceUri { get; protected set; }
		public JobTrackerState Status { get; set; }
		public IJobTask CurrentJob { get; protected set; }

		public Dictionary<int, DateTime> WorkerAliveSignals {
			get { return workerAliveSignals; }
			protected set { workerAliveSignals = value; }
		}

		public Queue<IJobTask> JobsQueue {
			get { return jobsQueue; }
			protected set { jobsQueue = value; }
		}

		protected JobTracker(Worker worker) {
			Status = JobTrackerState.Available;
			Worker = worker;
			ServiceUri = BuildUri();
		}

		public virtual void Run() {
			if (!Enabled)
				return;
			StateCheck();

			if (ServiceUri == null)
				ServiceUri = BuildUri();

			// register object
			RemotingServices.Marshal(this, GetServiceNamePrefix(), GetType());
		}

		//wakes requests frozen during frozen state
		private void ProcessFrozenRequests() {
			foreach (var mre in frozenRequests) {
				mre.Set();
			}
		}

		private string GetServiceNamePrefix() {
			return string.Format("{0}-W{1}"
				, (this is TaskRunner) ? PREF_MASTER_SVC_NAME : PREF_TRACKER_SVC_NAME
				, Worker.WorkerId);
		}

		private Uri BuildUri() {
			return new Uri(string.Format("tcp://{0}:{1}/{2}",
							Util.GetHostIpAddress(),
							Worker.ServiceUrl.Port,
							GetServiceNamePrefix()));
		}

		internal void Wake() {
			MainResetEvent.Set();
		}

		public void ScheduleJob(IJobTask job) {
			StateCheck();
			lock (TrackerMutex) {
				jobsQueue.Enqueue(job);
			}
		}

		public JobTrackerStateInfo GetState() {
			return new JobTrackerStateInfo() {
				CurrentJob = CurrentJob,
				ServiceUri = ServiceUri,
				Worker = Worker,
				Status = Status,
				Enabled = Enabled,
				JobsQueue = jobsQueue,
				WorkerAliveSignals = workerAliveSignals
			};
		}

		public void Alive(int wid) {
			StateCheck();
			Trace.WriteLine("Alive signal worker '" + wid + "'.");
			lock (TrackerMutex) {
				var w = ((Worker)Worker.GetWorkersList()[wid]);
				if (w.Status == WorkerStatus.Offline)
					w.SetStatus(WorkerStatus.Available);
				workerAliveSignals[wid] = DateTime.Now;
			}
		}

		public void FreezeCommunication() {
			JobTrackerState state;
			lock (TrackerMutex) {
				state = Status;
			}
			if (state == JobTrackerState.Frozen)
				return;
			lock (TrackerMutex) {
				Status = JobTrackerState.Frozen;
			}
			frozenRequests.Clear();
		}

		public void UnfreezeCommunication() {
			JobTrackerState state;
			lock (TrackerMutex) {
				state = Status;
			}
			if (state != JobTrackerState.Frozen)
				return;
			lock (TrackerMutex) {
				Status = JobTrackerState.Available;
			}
			ProcessFrozenRequests();
		}

		//puts to sleep all incoming requests while worker is frozen
		public void StateCheck() {
			JobTrackerState state;
			lock (TrackerMutex) {
				state = Status;
			}

			if (state != JobTrackerState.Frozen)
				return;

			var mre = new ManualResetEvent(false);
			frozenRequests.Add(mre);
			mre.WaitOne();
		}

		public void Dispose() {
			RemotingServices.Disconnect(this);
		}
	}
}