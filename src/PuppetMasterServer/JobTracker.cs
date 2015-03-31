using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedTypes;

namespace PlatformCore
{
    internal class JobTracker : MarshalByRefObject, IJobTracker
    {
        private Worker worker;

        public enum JobTrackerStatus { ACTIVE, PASSIVE };
        private JobTrackerStatus mode = JobTrackerStatus.PASSIVE;
        private List<IWorker> activeWorkers = new List<IWorker>();
        private DateTime lastHeartBeat = DateTime.UtcNow.Date;

        public JobTracker(Worker worker, JobTrackerStatus mode) {
            this.activeWorkers = worker.GetActiveWorkers();
            this.worker = worker;
            this.mode = mode;
        }

        internal void Start() {
            if (Enum.Equals(mode, JobTrackerStatus.ACTIVE)) {
            }
        }

        public void Alive(string wid) {

        }

        public void Complete(string wid) {

        }

        public void Beat() {
        }
    }
}