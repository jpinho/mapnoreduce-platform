using System;

namespace SharedTypes
{
    public interface IWorker
    {
        string GetWorkerURL();

        void ReceiveMapJob(string filePath, int nSplits, byte[] mapAssemblyCode, string mapClassName);

        bool ExecuteMapJob(IJobTask task);
    }
}