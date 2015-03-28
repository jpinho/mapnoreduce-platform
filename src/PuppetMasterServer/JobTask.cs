using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformCore
{
    public class JobTask : SharedTypes.IJobTask
    {
        public string FileName { get; set; }
        public string SplitNumber { get; set; }
        public string SplitProviderURL { get; set; }
        public string OutputReceiverURL { get; set; }
        public byte[] MapFunctionAssembly { get; set; }
        public string MapClassName { get; set; }
    }
}