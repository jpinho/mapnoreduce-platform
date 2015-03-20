using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppetMasterLib;

namespace ClientConnectorService
{
    public class ClientSplitProviderService : MarshalByRefObject, IClientSplitProviderService
    {
        public string GetFileSplit(string filename, int splitNumber) {
            return null;
        }
    }
}
