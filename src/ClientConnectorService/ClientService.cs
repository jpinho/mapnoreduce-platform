using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using SharedTypes;

namespace ClientConnectorService
{
    public class ClientService : MarshalByRefObject, IClientService
    {
        private ClientOutputReceiverService outputReceiverService;
        private ClientSplitProviderService splitProviderService;
        private bool isInitiated = false;

        public void Init(string entryURL) {
            if (isInitiated)
                return;

            // startup of client services
            isInitiated = true;
            outputReceiverService = new ClientOutputReceiverService();
            splitProviderService = new ClientSplitProviderService();

            RemotingServices.Marshal(outputReceiverService, outputReceiverService.ToString(), typeof(ClientOutputReceiverService));
            RemotingServices.Marshal(splitProviderService, splitProviderService.ToString(), typeof(ClientSplitProviderService));
        }

        public void Submit(string filePath, int nSplits, string outputDir, IMap mapFunction) {
            this.splitProviderService.SplitAndSave(filePath, nSplits);

            //TODO: There can be no file being processed twice at the same time. 
            // This is File1 cannot be processed if it is currently being processed (corner case!).
        }
    }
}
