using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PuppetMasterLib;

namespace ClientConnectorService
{
    public interface IClientService
    {
        void Init(string entryURL);
        void Submit(string filePath, int nSplits, string outputDir, IMap mapFunction);
    }
}
