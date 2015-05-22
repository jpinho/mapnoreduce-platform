using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
	[Serializable]
	public class TaskRunner : JobTracker
	{
		private CoordinationManager replicaManager;
		private bool isFistReplicationRun;

		public TaskRunner(Worker worker)
			: base(worker) {
		}

		public TaskRunner(Worker worker, JobTrackerStateInfo state)
			: base(worker) {

			if (state.CurrentJob != null)
				JobsQueue.Enqueue(state.CurrentJob);

			JobsQueue = new Queue<IJobTask>(JobsQueue.Union(state.JobsQueue));
			Enabled = state.Enabled;
			ServiceUri = state.ServiceUri;
			WorkerAliveSignals = state.WorkerAliveSignals;
			Status = state.Status;

			Worker.UpdateAvailableWorkers(
				Worker.GetIWorkerObjects(
					state.Worker.WorkersList.ConvertAll(input => input.ServiceUrl)
					));

			InReplicaState = true;
			ReplicatedWorkerId = state.WorkerId;
			isFistReplicationRun = true;
		}

		public override void Run() {
			base.Run();
			//Trace.WriteLine("TaskRunner starting CoordinationManager for fault tolerance.");

			while (Enabled) {
				Thread.Sleep(100);
				TrackJobs();
				lock (TrackerMutex) {
					if (JobsQueue.Count == 0)
						return;
				}
			}
		}

		public void AliveReplica(Uri workerUrl) {
			replicaManager.ReplicaAliveSignal(workerUrl);
		}

		private void TrackJobs() {
			lock (TrackerMutex) {
				if (JobsQueue.Count == 0)
					return;
				// Get next job and set state to busy.
				CurrentJob = JobsQueue.Dequeue();

				if (!(InReplicaState && isFistReplicationRun))
					PullAvailableWorkers();

				if (InReplicaState && isFistReplicationRun)
					isFistReplicationRun = false;

				replicaManager = new CoordinationManager(this);
				replicaManager.Start();
				Status = JobTrackerState.Busy;
			}

			var thrScheduler = new Thread(() => (new DefaultJobScheduler(this, CurrentJob)).Run());
			thrScheduler.Start();
			thrScheduler.Join();
			Worker.ReleaseWorkers();
			replicaManager.Dispose();

			lock (TrackerMutex) {
				CurrentJob = null;
				Status = JobTrackerState.Available;
			}
		}

		public void ReceiveShare(List<Uri> share) {
			Worker.UpdateAvailableWorkers(
				Worker.GetIWorkerObjects(share));
			WaitForShareEvent.Set();
		}

		public void PullAvailableWorkers() {
			var pMaster = (IPuppetMasterService)Activator.GetObject(
				typeof(IPuppetMasterService),
				PuppetMasterService.ServiceUrl.ToString());
			try {
				Worker.UpdateAvailableWorkers(
					Worker.GetIWorkerObjects(
						pMaster.GetWorkersShare(ServiceUri)));
			} catch (System.Exception e) {
				Trace.WriteLine(e.Message);
			} finally {
				if (!(Worker.GetWorkersList().Count > 0)) {
					Worker.SetStatus(WorkerStatus.Busy);
					WaitForShareEvent.WaitOne();
				}
			}
		}

		public override void FreezeCommunication() {
			base.FreezeCommunication();
			replicaManager.PauseStateUpdates();
		}

		public override void UnfreezeCommunication() {
			base.UnfreezeCommunication();
			replicaManager.ResumeStateUpdates();
		}
	}
}