using System;
using System.Collections.Generic;

namespace SharedTypes
{
    public interface ISlaveReplica
    {
        IWorker Worker { get; set; }
        Tuple<JobTrackerStateInfo, DateTime> MasterJobTrackerState { get; set; }
        bool Enabled { get; set; }
        int Priority { get; set; }
        List<ISlaveReplica> Siblings { get; set; }
    }
}