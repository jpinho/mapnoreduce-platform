using System;
using System.Collections.Generic;

namespace PlatformCore
{
	[Serializable]
	public class JobTask : SharedTypes.IJobTask
	{
		public Uri JobTrackerUri { get; set; }
		public string FileName { get; set; }
		public List<int> FileSplits { get; set; }
		public int SplitNumber { get; set; }
		public string SplitProviderUrl { get; set; }
		public string OutputReceiverUrl { get; set; }
		public byte[] MapFunctionAssembly { get; set; }
		public string MapClassName { get; set; }

		public JobTask() {
		}

		public object Clone() {
			var newTask = (JobTask)MemberwiseClone();
			newTask.SplitNumber = -1;
			return newTask;
		}
	}
}