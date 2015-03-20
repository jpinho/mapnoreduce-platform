using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedTypes
{
    public interface IClientService
    {
        void Init(string entryURL);
        void Submit(string filePath, int nSplits, string outputDir, IMap mapFunction);
    }
}
