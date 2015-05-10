using System;
using System.Collections.Generic;
namespace SharedTypes
{
	public interface IWorker
	{
		Uri ServiceUrl { get; set; }
		int WorkerId { get; set; }

		bool ExecuteMapJob(IJobTask task);
		void ReceiveMapJob(IJobTask job);
		WorkerStatus GetStatus();
		void SetStatus(WorkerStatus status);
		void Slow(int secs);
		void Freeze();
		void UnFreeze();
		void FreezeCommunication();
		void UnfreezeCommunication();
		void UpdateAvailableWorkers(Dictionary<int, IWorker> availableWorkers);
        void NotifyWorkerJoin(Uri uri);

		void AsyncExecuteMapJob(int split,
			string fileName, List<int> fileSplits, Uri jobTrackerUri, string mapClassName,
			byte[] mapFunctionName, string outputReceiverUrl, string splitProviderUrl);

    }
}