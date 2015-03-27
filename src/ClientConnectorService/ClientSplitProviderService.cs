using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTypes;

namespace ClientServices
{
    internal class ClientSplitProviderService : MarshalByRefObject, IClientSplitProviderService
    {
        private Dictionary<string, List<string>> splitsStore = new Dictionary<string, List<string>>();
        public string ServiceURL { get; private set; }

        public ClientSplitProviderService(string serviceURL) {
            ServiceURL = serviceURL;
        }

        public string GetFileSplit(string filePath, int splitNumber) {
            return splitsStore[filePath][splitNumber - 1];
        }

        internal void SplitAndSave(string filePath, int nSplits) {
            List<string> lstSplits = new List<string>();
            StreamReader sr = new StreamReader(filePath);

            string[] lines = sr.ReadToEnd().Split('\n');
            int splitSize = (int)Math.Floor((double)lines.Length / (double)nSplits);

            // performs the attributions of lines to their splits
            List<string> splitContent = new List<string>();
            foreach (string line in lines) {
                // adds the line to the current split
                splitContent.Add(line);

                // split size reached
                if (splitContent.Count >= splitSize) {
                    // saves the split
                    lstSplits.Add(String.Join("\n", splitContent));
                    // the next cycle is the new split
                    splitContent = new List<string>();
                }
            }

            // adds last split (if incomplete)
            if (splitContent.Count >= 0)
                lstSplits.Add(String.Join("\n", splitContent));

            // saves the splits of the file on the store
            splitsStore.Add(filePath, lstSplits);
        }
    }
}