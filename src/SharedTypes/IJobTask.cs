using System;
using System.Collections.Generic;

namespace SharedTypes
{
    /// <summary>
    /// This DTO represents the information required by a Worker to process a job split or to
    /// distribute it to multiple workers.
    /// </summary>
    public interface IJobTask : ICloneable
    {
        /// <summary>
        /// The Uri of the Job Tracker in charge of coordinating the job. Slave Job Trackers must
        /// notify the Master Job Tracker within a specified TIMEOUT to keep the job split on their side.
        /// </summary>
        Uri JobTrackerUri { get; set; }

        /// <summary>
        /// The name of the file being processed.
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// The individual splits to be processed in this job.
        /// </summary>
        List<int> FileSplits { get; set; }

        /// <summary>
        /// The number of the split being processed in this job task. When set to '-1' it means that
        /// the job split of this task has not been defined.
        /// </summary>
        int SplitNumber { get; set; }

        /// <summary>
        /// The Split Provider URL from where split data can be obtained.
        /// </summary>
        string SplitProviderUrl { get; set; }

        /// <summary>
        /// The Output Receiver URL to where results should be sent.
        /// </summary>
        string OutputReceiverUrl { get; set; }

        /// <summary>
        /// The assembly byte code containing the Map Class.
        /// </summary>
        byte[] MapFunctionAssembly { get; set; }

        /// <summary>
        /// The name of the Map Class in the <see cref="MapFunctionAssembly"/>.
        /// </summary>
        string MapClassName { get; set; }
    }
}