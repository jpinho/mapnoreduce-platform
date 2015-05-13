using System;
using System.Collections.Generic;

namespace SharedTypes {
    public interface IPuppetMasterService {
        void CreateWorker(int workerId, string serviceUrl, string entryUrl);

        void GetStatus();

        void Wait(int seconds);

        void SlowWorker(int workerId, int seconds);

        void FreezeWorker(int workerId);

        void UnfreezeWorker(int workerId);

        void FreezeCommunication(int workerId);

        void UnfreezeCommunication(int workerId);

        Dictionary<int, IWorker> GetWorkers();

        Dictionary<int, IWorker> GetWorkersShare(Uri taskRunnerUri);

        void ReleaseWorkers(List<int> workersUsed);

        Uri GetServiceUri();

        void AnnouncePM(Uri puppetMasterUri);

        Dictionary<int, IWorker> GetWorkersSharePM(Uri pmUri);
    }
}