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
        private object p;

        public enum JobTrackerStatus { ACTIVE, PASSIVE };
        private List<IWorker> activeWorkers = new List<IWorker>();

        public JobTracker(Worker worker, JobTrackerStatus p) {
            this.worker = worker;
            this.p = p;
            this.activeWorkers = worker.GetActiveWorkers();
        }

        internal void Start() {
            if (Enum.Equals(p, JobTrackerStatus.ACTIVE)) {
            }
        }

        public void alive(string wid) {

        }

        public void complete(string wid) {

        }
    }
}