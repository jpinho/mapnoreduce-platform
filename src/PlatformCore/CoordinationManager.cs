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

		private readonly Timer statusUpdatesTimer;
		private volatile JobTracker tracker;
		private List<IWorker> replicas;
		private readonly Dictionary< /*workerid*/ int, /*lastping*/ DateTime> replicasAliveSignals;
		private readonly object rmanagerMutex = new object();
		private bool isStarted = false;

		public int NumberOfReplicas { get; set; }

		public CoordinationManager(JobTracker tracker) {
			NumberOfReplicas = DEFAULT_NUMBER_OF_REPLICAS;
			this.tracker = tracker;
			replicasAliveSignals = new Dictionary<int, DateTime>();
			statusUpdatesTimer = new Timer(StatusUpdate, 0, Timeout.Infinite, STATUS_UPDATE_TIMEOUT);
		}

		//TODO: Nominate 30% of the share assigned to the JobTracker to be replica workers (slave trackers).
		private List<IWorker> PickReplicas() {
			Trace.WriteLine("CoordinatorManager picking replicas for fault tolerance.");
			/* #begin# temporary algorithm - stoles some workers to set as replicas */
			var puppetMaster = RemotingHelper.GetRemoteObject<PuppetMasterService>(PuppetMasterService.ServiceUrl);
			var reps = (from wk in puppetMaster.GetAvailableWorkers().Take(NumberOfReplicas) select wk.Value).ToList();
			/* #end# */
			Trace.WriteLine("CoordinatorManager just picked " + reps.Count + " replicas from PuppetMaster.");
			return reps;
		}

		public void Start() {
			if (isStarted)
				return;
			isStarted = true;
			replicas = PickReplicas();
			replicas.AsParallel().ForAll((wk) => {
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

		private void StatusUpdate(object state) {
			foreach (var replica in replicas) {
				try {
					Trace.WriteLine("CoordinationManager sending JobTracker state to ReplicaWorker ID:" + replica.WorkerId + ".");
					replica.ReceiveJobTrackerState(tracker.GetState());
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
						while (!RecoverCrashedReplica())
							Thread.Sleep(REPLICA_RECOVER_ATTEMPT_DELAY);
					}).Start();

				}
			}
		}

		private bool RecoverCrashedReplica() {
			var puppetMaster = RemotingHelper.GetRemoteObject<PuppetMasterService>(PuppetMasterService.ServiceUrl);
			var reps = (from wk in puppetMaster.GetAvailableWorkers().Take(1) select wk.Value).ToList();

			if (reps.Count == 0) {
				Trace.WriteLine("RecoverCrashedReplica failed to acquire a new replica.");
				return false;
			}
			var replica = reps.First();
			replica.ReceiveJobTrackerState(tracker.GetState());
			replicas.Add(replica);
			return true;
		}

		public void Dispose() {
			statusUpdatesTimer.Dispose();
			replicas.AsParallel().ForAll((worker) => {
				worker.DestroyReplica();
			});
		}
	}
}