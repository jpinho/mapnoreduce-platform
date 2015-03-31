﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedTypes;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Collections;

namespace PlatformCore
{
    internal class JobTracker : MarshalByRefObject, IJobTracker
    {
        private Worker worker;

        public enum JobTrackerStatus { ACTIVE, PASSIVE };
        private JobTrackerStatus mode = JobTrackerStatus.PASSIVE;
        private Dictionary<int, IWorker> activeWorkers = new Dictionary<int, IWorker>();
        private DateTime lastHeartBeat = DateTime.UtcNow.Date;

        public JobTracker(Worker worker)
        {
            this.worker = worker;
        }

        public void Start(JobTrackerStatus trackerMode)
        {
            // Register Services (alive, complete)
            // Do Work
            if (Enum.Equals(trackerMode, JobTrackerStatus.ACTIVE))
            {
                RemotingServices.Marshal(this, "JobTracker", typeof(IJobTracker));
            }
        }

        public void Alive(int wid)
        {
            /*TODO fault tolerant algorithm*/
        }

        public void Complete(int wid)
        {
            //Remove worker from active list
            activeWorkers.Remove(wid);
        }
    }
}
