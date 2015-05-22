using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedTypes
{
    /// <summary>
    /// Interface used by all machines which process jobs, either for distributing the Job as Master
    /// or executing some task (part of a job) as commun Worker.
    /// </summary>
    public interface IWorker
    {
        /// <summary>
        /// The service URL used to reach this work remotely.
        /// </summary>
        Uri ServiceUrl { get; set; }

        /// <summary>
        /// The Uri of the Puppet Master who created this worker.
        /// </summary>
        Uri PuppetMasterUri { get; set; }

        /// <summary>
        /// This worker id.
        /// </summary>
        int WorkerId { get; set; }

		/// <summary>
		/// The thread that executes the job. Usefull for freeze and slow.
		/// </summary>
		Task ExecutionTask { get; set; }

        /// <summary>
        /// List of all workers known by this worker.
        /// </summary>
        List<IWorker> WorkersList { get; set; }

        /// <summary>
        /// Function responsible to executes the task received: Asks for the data needed to the
        /// client, loads the map function and applies it. Sends the result back to the client.
        /// </summary>
        /// <param name="task">the task to be executed</param>
        void ExecuteMapJob(IJobTask task);

        /// <summary>
        /// Receives a Map job and distributes the processing of their contents accross multiple
        /// workers. Each worker processes a split of the given filePath (used as file ID), calling
        /// the Map method of the received class name of the assembly code.
        /// </summary>
        /// <param name="job">The job to be processed.</param>
        void ReceiveMapJob(IJobTask job);

        /// <summary>
        /// Provides the status of this worker.
        /// </summary>
        WorkerStatus GetStatus();

        /// <summary>
        /// Sets the status of this worker to status received.
        /// </summary>
        /// <param name="status">the status to set</param>
        void SetStatus(WorkerStatus status);

        /// <summary>
        /// Introduces a delay of the given seconds into the worker thread.
        /// </summary>
        /// <param name="secs">seconds to delay</param>
        void Slow(int secs);

        /// <summary>
        /// Simulates a fault of this worker. Changes the status to frozen and freezes all the
        /// requests using ManualResetEvents.
        /// </summary>
        /// <see><cref>https://msdn.microsoft.com/en-us/library/system.threading.manualresetevent</cref></see>
        void Freeze();

        /// <summary>
        /// Simulates recovery from a fault of this worker. Changes the status to available and
        /// processes all the frozen requests setting the ManualResetEvents.
        /// </summary>
        /// <see><cref>https://msdn.microsoft.com/en-us/library/system.threading.manualresetevent</cref></see>
        void UnFreeze();

        /// <summary>
        /// Simulates a fault of this worker's MasterTracker component.
        /// </summary>
        void FreezeCommunication();

        /// <summary>
        /// Simulates recovery from a fault of this worker's MasterTracker component.
        /// </summary>
        void UnfreezeCommunication();

        /// <summary>
        /// Updates the list of available workers to the one that receives.
        /// </summary>
        /// <param name="availableWorkers">new available workers list</param>
        void UpdateAvailableWorkers(List<IWorker> availableWorkers);

        /// <summary>
        /// Retrives a list of IWorker Objects from the list of workers uris received.
        /// </summary>
        /// <param name="workersList">list of Uris of the workers known</param>
        List<IWorker> GetIWorkerObjects(List<Uri> workersList);

        /// <summary>
        /// Adds a new worker to the known workers list.
        /// </summary>
        /// <param name="uri">Uri of the new worker</param>
        void NotifyWorkerJoin(Uri uri);

        /// <summary>
        /// Produces the DTO of the JobTask to execute
        /// </summary>
        /// <param name="split">split identifier</param>
        /// <param name="fileName">file to read</param>
        /// <param name="fileSplits">splits' list</param>
        /// <param name="jobTrackerUri">jobTracker's Uri</param>
        /// <param name="mapClassName">Map class</param>
        /// <param name="mapFunctionName">Map function</param>
        /// <param name="outputReceiverUrl">Url to delivery results</param>
        /// <param name="splitProviderUrl">Url to ask for the split</param>
        void ExecuteMapJob(int split,
            string fileName, List<int> fileSplits, Uri jobTrackerUri, string mapClassName,
            byte[] mapFunctionName, string outputReceiverUrl, string splitProviderUrl);

        /// <summary>
        /// Receives the state from JobTracker
        /// </summary>
        /// <param name="getState">state to receive</param>
        void ReceiveJobTrackerState(JobTrackerStateInfo getState);

        /// <summary>
        /// Destroys this worker's replica.
        /// </summary>
        void DestroyReplica();

        /// <summary>
        /// Starts a replica trackers with the given priority.
        /// </summary>
        /// <param name="priority">
        /// Priority used to decide which replica would rise in case of failure.
        /// </param>
        ISlaveReplica StartReplicaTracker(int priority);

        /// <summary>
        /// Sends this worker/replica sate to another replica.
        /// </summary>
        /// <param name="slaveReplica">Replica which will receive the state.</param>
        void SendReplicaState(ISlaveReplica slaveReplica);

        /// <summary>
        /// Promotes a replica to Master based on the masterJobTrackerStateInfo
        /// </summary>
        /// <param name="masterJobTrackerState">Information from the masterJobTracker</param>
        void PromoteToMaster(JobTrackerStateInfo masterJobTrackerState);

        /// <summary>
        /// Updates the list of replicas to the given list
        /// </summary>
        /// <param name="replicasGroup">new list of replicas</param>
        void UpdateReplicas(List<ISlaveReplica> replicasGroup);

        /// <summary>
        /// Ensures a SlaveTracker to handle state updates from Master Tracker.
        /// </summary>
        ISlaveReplica EnsureReplicaTracker();
    }
}