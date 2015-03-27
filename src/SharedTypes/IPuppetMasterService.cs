using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedTypes
{
    public interface IPuppetMasterService
    {
        void CreateWorker(int workerId, string serviceURL, string entryURL);

        Dictionary<int, IWorker> GetWorkers();
    }
}