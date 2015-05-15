using System;
using System.Collections.Generic;

namespace SharedTypes
{
    public interface IPuppetMasterService
    {
        void CreateWorker(int workerId, string serviceUrl, string entryUrl);

        void GetStatus();

        void SlowWorker(int workerId, int seconds);

        void FreezeWorker(int workerId);

        void UnfreezeWorker(int workerId);

        void FreezeCommunication(int workerId);

        void UnfreezeCommunication(int workerId);

        Dictionary<int, IWorker> GetAvailableWorkers();

        List<Uri> GetWorkersShare(Uri taskRunnerUri);

        void ReleaseWorkers(List<int> workersUsed);

        Uri GetServiceUri();

        void BroadcastAnnouncePm(Uri newPuppetMasterUri);

        List<Uri> GetJobTrackersMaster();

        int GetJobTrackersMasterCount();

        List<Uri> UpdatePmsList(List<Uri> puppetMasterUrls);

        List<Uri> GetWorkersSharePm(Uri pmUri);
    }
}