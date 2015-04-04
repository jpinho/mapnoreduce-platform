using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedTypes
{
    public interface IPuppetMasterService
    {
        void CreateWorker(int workerId, string serviceUrl, string entryURL);

        void GetStatus();

        void Wait(int seconds);

        void SlowWorker(string WorkerId, int seconds);

        void FreezeWorker(string WorkerId);

        void UnfreezeWorker(string WorkerId);

        void FreezeCommunication(string WorkerId);

        void UnfreezeCommunication(string WorkerId);

        Dictionary<int, IWorker> GetWorkers();
    }
}