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

        void AnnouncePm(Uri newPuppetMasterUri, bool broadcast = false);

        void AnnouncePm(List<Uri> puppetMasterUrls, bool broadcast = false);

        List<Uri> GetWorkersSharePm(Uri pmUri);
    }
}