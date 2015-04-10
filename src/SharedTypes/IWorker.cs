using System;
using System.Collections.Generic;
namespace SharedTypes
{
    public interface IWorker
    {
        Uri ServiceUrl { get; set; }
        int WorkerId { get; set; }

        bool ExecuteMapJob(IJobTask task);
        void ReceiveMapJob(IJobTask job);
        void GetStatus();
        void Slow(int secs);
        void Freeze();
        void UnFreeze();
        void FreezeCommunication();
        void UnfreezeCommunication();
        void UpdateAvailableWorkers(Dictionary<int, IWorker> availableWorkers);
        void AsyncExecuteMapJob(IJobTracker jobTracker, int split, IWorker remoteWorker, AsyncCallback callback, IJobTask job);
    }
}