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
        void UpdateAvailableWorkers(Dictionary<int, IWorker> availableWorkers);
    }
}