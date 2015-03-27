using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTypes;

namespace ClientServices
{
    internal class ClientOutputReceiverService : MarshalByRefObject, IClientOutputReceiverService
    {
        public string ServiceURL { get; private set; }

        public ClientOutputReceiverService(string serviceURL) {
            ServiceURL = serviceURL;
        }

        private Dictionary<string, List<KeyValuePair<int, string>>> mapResultStore = new Dictionary<string, List<KeyValuePair<int, string>>>();

        public void ReceiveMapOutputFragment(string filePath, string result, int splitNumber) {
            if (!mapResultStore.ContainsKey(filePath))
                mapResultStore.Add(filePath, new List<KeyValuePair<int, string>>());

            mapResultStore[filePath].Add(new KeyValuePair<int, string>(splitNumber, result));
            //TODO: trigger event to notify the receival of a new result from a file and to alert that the worker finished that split.
        }
    }
}