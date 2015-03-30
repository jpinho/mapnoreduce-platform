using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedTypes
{
    public interface IClientService
    {
        void Init(string entryUrl);

        void Submit(string filePath, int nSplits, string outputDir, string mapClassName, string assemblyFilePath);
    }
}