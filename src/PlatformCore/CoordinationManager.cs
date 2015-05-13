using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
	[Serializable]
	public class CoordinationManager : IDisposable
	{
		private const int STATUS_UPDATE_TIMEOUT = 10 * 1000;
		private const int REPLICA_RECOVER_ATTEMPT_DELAY = 10 * 1000;
		public const int DEFAULT_NUMBER_OF_REPLICAS = 3;
		private const double REPLICATION_FACTOR = 2;

		private readonly Timer statusUpdatesTimer;
		private volatile JobTracker tracker;
		private List<IWorker> replicas;
		private readonly Dictionary< /*workerid*/ int, /*lastping*/ DateTime> replicasAliveSignals;
		private readonly object rmanagerMutex = new object();
		private bool isStarted;

		public int NumberOfReplicas { get; set; }

		public CoordinationManager(JobTracker tracker) {
			NumberOfReplicas = DEFAULT_NUMBER_OF_REPLICAS;
			this.tracker = tracker;
			replicasAliveSignals = new Dictionary<int, DateTime>();
			statusUpdatesTimer = new Timer(StatusUpdate, 0, Timeout.Infinite, STATUS_UPDATE_TIMEOUT);
		}

		private List<IWorker> PickReplicas() {
			Trace.WriteLine("CoordinatorManager picking replicas for fault tolerance.");

			var repsCount = GetWiseNumberForReplicas(tracker.Worker.GetWorkersList().Count);
			var reps = (from wk in tracker.Worker.GetWorkersList().Take(repsCount) select wk.Value).ToList();

			Trace.WriteLine("CoordinatorManager just picked " + repsCount + " replicas from PuppetMaster.");
			return reps;
		}

		private static int GetWiseNumberForReplicas(int x) {
			return Convert.ToInt32(Math.Round(Math.Ceiling(Math.Log(x, 2)) * REPLICATION_FACTOR, 0));
		}

		public void Start() {
			if (isStarted)
				return;
			isStarted = true;
			replicas = PickReplicas();
			replicas.ForEach(wk => {
				// ReSharper disable once InconsistentlySynchronizedField
				replicasAliveSignals[wk.WorkerId] = DateTime.Now;
			});
			statusUpdatesTimer.Change(0, STATUS_UPDATE_TIMEOUT);
		}

		public void ReplicaAliveSignal(int workerId) {
			lock (rmanagerMutex) {
				replicasAliveSignals[workerId] = DateTime.Now;
				Trace.WriteLine("CoordinationManager received ping signal from replica/worker ID:" + workerId
					+ " at '" + DateTime.Now.ToString("ddMMyyyyTHH:mm:ss:fff") + "'.");
			}
		}

		public void PauseStateUpdates() {
			statusUpdatesTimer.Change(Timeout.Infinite, STATUS_UPDATE_TIMEOUT);
		}

		public void ResumeStateUpdates() {
			statusUpdatesTimer.Change(0, STATUS_UPDATE_TIMEOUT);
		}

		private void StatusUpdate(object state) {
			foreach (var replica in replicas) {
				try {
					Trace.WriteLine("CoordinationManager sending JobTracker state to ReplicaWorker ID:"
						+ replica.WorkerId + ".");
					RemotingHelper.GetRemoteObject<IWorker>(replica.ServiceUrl)
						.ReceiveJobTrackerState(tracker.GetState());
				} catch {
					Trace.WriteLine("CoordinationManager received an error contacting replica.");
					TimeSpan lastReplicaUpdate;

					lock (rmanagerMutex) {
						lastReplicaUpdate = DateTime.Now.Subtract(replicasAliveSignals[replica.WorkerId]);
					}

					if (!(lastReplicaUpdate.TotalSeconds > SlaveReplica.PING_DELAY * 3))
						continue;

					Trace.WriteLine("CoordinatorManager detected that replica/worker ID:"
						+ replica.WorkerId + " seems to be permanently crashed.");

					new Thread(() => {
						while (!RecoverCrashedReplica()) {
							Trace.WriteLine("Replica crashed and not recovered, waiting...");
							Thread.Sleep(REPLICA_RECOVER_ATTEMPT_DELAY);
						}
					}).Start();
				}
			}
		}

		private bool RecoverCrashedReplica() {
			if (tracker.Worker.GetWorkersList().Count <= 1)
				return false;

			Trace.WriteLine("Recovering crashed replica.");
			var puppetMaster = RemotingHelper.GetRemoteObject<PuppetMasterService>(PuppetMasterService.ServiceUrl);
			var reps = (from wk in puppetMaster.GetAvailableWorkers().Take(1) select wk.Value).ToList();

			if (reps.Count == 0) {
				Trace.WriteLine("RecoverCrashedReplica failed to acquire a new replica.");
				return false;
			}

			Trace.WriteLine("Crashed replica recovered.");
			var replica = reps.First();
			replicas.Add(replica);
			lock (rmanagerMutex) {
				replicasAliveSignals[replica.WorkerId] = DateTime.Now;
			}
			RemotingHelper.GetRemoteObject<IWorker>(replica.ServiceUrl)
				.ReceiveJobTrackerState(tracker.GetState());
			return true;
		}

		public void Dispose() {
			Trace.WriteLine("CoordinationManager disposing.");
			statusUpdatesTimer.Dispose();
			replicas.ForEach(worker => {
				RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl)
					.DestroyReplica();
			});
		}
	}
}