using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using PuppetMasterLib;

namespace ClientConnectorService
{
    public class ClientService : MarshalByRefObject, IClientService
    {
        public void Init(string entryURL) {
        }

        public void Submit(string filePath, int nSplits, string outputDir, IMap mapFunction) { 
        }
    }
}
