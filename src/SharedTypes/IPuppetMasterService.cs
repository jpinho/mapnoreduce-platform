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

        Dictionary<int, IWorker> GetWorkers();
    }
}