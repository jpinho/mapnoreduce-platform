using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using SharedTypes;

namespace PlatformCore
{
    public class JobTracker : MarshalByRefObject, IJobTracker
    {
        private Worker worker;

        public enum JobTrackerStatus { ACTIVE, PASSIVE };
        private JobTrackerStatus mode = JobTrackerStatus.PASSIVE;
        private Dictionary<int, IWorker> activeWorkers = new Dictionary<int, IWorker>();
        private DateTime lastHeartBeat = DateTime.UtcNow.Date;

        public JobTracker(Worker worker) {
            this.worker = worker;
        }

        public void Start(JobTrackerStatus trackerMode) {
            // Register Services (alive, complete) Do Work
            if (Enum.Equals(trackerMode, JobTrackerStatus.ACTIVE)) {
                RemotingServices.Marshal(this, "JobTracker", typeof(IJobTracker));
            }
        }

        public void Alive(int wid) {
            /*TODO fault tolerant algorithm*/
        }

        public void Complete(int wid) {
            //Remove worker from active list
            activeWorkers.Remove(wid);
        }
    }
}