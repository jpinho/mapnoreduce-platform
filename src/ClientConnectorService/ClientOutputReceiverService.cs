using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTypes;

namespace ClientConnectorService
{
    public class ClientOutputReceiverService : MarshalByRefObject, IClientOutputReceiverService
    {
        public void ReceiveMapOutputFragment(string filename, string result, int splitNumber) {
        }
    }
}
