using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
	[Serializable]
	public class CoordinationManager
	{
		private const int STATUS_UPDATE_TIMEOUT = 10000;
		public const int DEFAULT_NUMBER_OF_REPLICAS = 5;

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

		private List<IWorker> PickReplicas() {
			Trace.WriteLine("CoordinatorManager picking replicas for fault tolerance.");
			/* #begin# temporary algorithm - stoles some workers to set as replicas */
			var puppetMaster = RemotingHelper.GetRemoteObject<PuppetMasterService>(PuppetMasterService.ServiceUrl);
			var reps = (from wk in puppetMaster.GetWorkers().Take(NumberOfReplicas) select wk.Value).ToList();
			/* #end# */
			Trace.WriteLine("CoordinatorManager just picked " + reps.Count + " replicas from PuppetMaster.");
			return reps;
		}

		public void Start() {
			if (isStarted)
				return;
			isStarted = true;
			replicas = PickReplicas();
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
					var lastReplicaUpdate = DateTime.Now.Subtract(replicasAliveSignals[replica.WorkerId]);

					if (!(lastReplicaUpdate.TotalSeconds > SlaveTracker.PING_DELAY * 3))
						continue;

					Trace.WriteLine("CoordinatorManager detected that replica/worker ID:"
						+ replica.WorkerId + " seems to be permanently crashed.");
					RecoverCrashedReplica();
				}
			}
		}

		private void RecoverCrashedReplica() {
			//TODO: Ask PuppetMaster for another replica.
		}
	}
}