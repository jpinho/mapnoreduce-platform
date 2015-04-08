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

        void SlowWorker(int WorkerId, int seconds);

        void FreezeWorker(int WorkerId);

        void UnfreezeWorker(int WorkerId);

        void FreezeCommunication(int WorkerId);

        void UnfreezeCommunication(int WorkerId);

        Dictionary<int, IWorker> GetWorkers();
    }
}