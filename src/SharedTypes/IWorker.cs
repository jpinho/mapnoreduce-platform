using System;
using System.Collections.Generic;

namespace SharedTypes
{
    public interface IWorker
    {
        Uri ServiceUrl { get; set; }
        Uri PuppetMasterUri { get; set; }
        int WorkerId { get; set; }
        Dictionary<int, IWorker> WorkersList { get; set; }

        void ExecuteMapJob(IJobTask task);
        void ReceiveMapJob(IJobTask job);
        WorkerStatus GetStatus();
        void SetStatus(WorkerStatus status);
        void Slow(int secs);
        void Freeze();
        void UnFreeze();
        void FreezeCommunication();
        void UnfreezeCommunication();
        void UpdateAvailableWorkers(Dictionary<int, IWorker> availableWorkers);
        Dictionary<int, IWorker> GetIWorkerObjects(List<Uri> workersList);
        void NotifyWorkerJoin(Uri uri);

        void ExecuteMapJob(int split,
            string fileName, List<int> fileSplits, Uri jobTrackerUri, string mapClassName,
            byte[] mapFunctionName, string outputReceiverUrl, string splitProviderUrl);

        void ReceiveJobTrackerState(JobTrackerStateInfo getState);
        void DestroyReplica();

        ISlaveReplica StartReplicaTracker(int priority);
        void SendReplicaState(ISlaveReplica slaveReplica);
        void PromoteToMaster(JobTrackerStateInfo masterJobTrackerState);
        void UpdateReplicas(List<ISlaveReplica> replicasGroup);
        ISlaveReplica EnsureReplicaTracker();
    }
}