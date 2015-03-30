using System;

namespace SharedTypes
{
    public interface IWorker
    {
        Uri ServiceUrl { get; set; }
        int WorkerId { get; set; }

        bool ExecuteMapJob(IJobTask task);
        void ReceiveMapJob(string filePath, int nSplits, byte[] mapAssemblyCode, string mapClassName);
    }
}