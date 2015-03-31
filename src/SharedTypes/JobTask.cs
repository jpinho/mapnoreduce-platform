using System.Collections.Generic;

namespace PlatformCore
{
    public class JobTask : SharedTypes.IJobTask
    {
        public string FileName { get; set; }
        public List<int> FileSplits { get; set; }
        public int SplitNumber { get; set; }
        public string SplitProviderUrl { get; set; }
        public string OutputReceiverUrl { get; set; }
        public byte[] MapFunctionAssembly { get; set; }
        public string MapClassName { get; set; }

        public object Clone() {
            JobTask newTask = (JobTask)this.MemberwiseClone();
            newTask.SplitNumber = -1;
            return newTask;
        }
    }
}