using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientConnectorService
{
    public interface IClientOutputReceiverService
    {
        void ReceiveMapOutputFragment(string filename, string result, int splitNumber);
    }
}
