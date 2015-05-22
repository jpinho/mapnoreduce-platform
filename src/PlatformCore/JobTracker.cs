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

		protected bool InReplicaState;
		protected int ReplicatedWorkerId;
		private volatile Dictionary</*workerid*/ int, /*lastupdate*/DateTime> workerAliveSignals = new Dictionary<int, DateTime>();
		private readonly List<ManualResetEvent> frozenRequests = new List<ManualResetEvent>();
		private Queue<IJobTask> jobsQueue = new Queue<IJobTask>();
		protected readonly object TrackerMutex = new object();

		public volatile bool Enabled = true;
		public Worker Worker { get; set; }
		public AutoResetEvent MainResetEvent = new AutoResetEvent(false);
		public AutoResetEvent WaitForShareEvent = new AutoResetEvent(false);

		public bool IsProxyConnected { get; private set; }
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
			IsProxyConnected = false;
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
			ConnectProxy();
		}

		private string GetServiceNamePrefix() {
			return string.Format("{0}-W{1}"
				, (this is TaskRunner) ? PREF_MASTER_SVC_NAME : PREF_TRACKER_SVC_NAME
				, InReplicaState ? ReplicatedWorkerId : Worker.WorkerId);
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
				WorkerId = InReplicaState ? ReplicatedWorkerId : Worker.WorkerId,
				Status = Status,
				Enabled = Enabled,
				JobsQueue = jobsQueue,
				WorkerAliveSignals = workerAliveSignals,
				Worker = Worker
			};
		}

		public void Alive(int wid) {
			StateCheck();
			//Trace.WriteLine("Alive signal worker '" + wid + "'.");
			lock (TrackerMutex) {
				var w = ((Worker)Worker.GetWorkersList()[wid]);
				if (w.Status == WorkerStatus.Offline)
					w.SetStatus(WorkerStatus.Available);
				workerAliveSignals[wid] = DateTime.Now;
			}
		}

		public virtual void FreezeCommunication() {
			lock (TrackerMutex) {
				Status = JobTrackerState.Frozen;
				DisconnectProxy();
				Trace.WriteLine("Communication Freezed for worker '" + (InReplicaState ? ReplicatedWorkerId : Worker.WorkerId) + "'.");
			}
		}

		public virtual void UnfreezeCommunication() {
			lock (TrackerMutex) {
				Status = CurrentJob != null ? JobTrackerState.Busy : JobTrackerState.Available;
				ConnectProxy();
				ProcessFrozenRequests();
				Trace.WriteLine("Communication Unfreezed for worker '" + (InReplicaState ? ReplicatedWorkerId : Worker.WorkerId) + "'.");
			}
		}

		private void ProcessFrozenRequests() {
			frozenRequests.ForEach(r => r.Set());
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

		public void ConnectProxy() {
			//Trace.WriteLine("JobTracker of type '" + GetType().FullName + "' being connected.");
			IsProxyConnected = true;
			RemotingServices.Marshal(this, GetServiceNamePrefix(), GetType());
		}

		public void DisconnectProxy() {
			//Trace.WriteLine("JobTracker of type '" + GetType().FullName + "' being disconnected.");
			IsProxyConnected = false;
			RemotingServices.Disconnect(this);
		}

		public void Dispose() {
			DisconnectProxy();
		}
	}
}