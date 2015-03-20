using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientConnectorService
{
    public interface IClientSplitProviderService
    {
        string GetFileSplit(string filename, int splitNumber);
    }
}
