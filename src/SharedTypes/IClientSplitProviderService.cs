using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedTypes
{
    public interface IClientSplitProviderService
    {
        string GetFileSplit(string filename, int splitNumber);
    }
}
