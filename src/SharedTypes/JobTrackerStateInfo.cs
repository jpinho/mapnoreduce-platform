using System;
using System.Collections.Generic;

namespace SharedTypes
{
	[Serializable]
	public class JobTrackerStateInfo
	{
		public Dictionary<int, DateTime> WorkerAliveSignals;
		public Queue<IJobTask> JobsQueue;
		public Uri ServiceUri;
		public JobTrackerState Status;
		public IJobTask CurrentJob;
		public bool Enabled;
		public IWorker Worker;
	}
}