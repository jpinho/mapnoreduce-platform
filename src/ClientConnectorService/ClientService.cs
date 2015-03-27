using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using SharedTypes;

namespace ClientServices
{
    public class ClientService : MarshalByRefObject, IClientService
    {
        public const int CLIENT_CHANNEL_PORT = 8090;
        public const string CLIENT_OUTPUTRECV_SVCNAME = "MNRP-ClientORS";
        public const string CLIENT_SPLITPROV_SVCNAME = "MNRP-ClientSPS";

        private ClientOutputReceiverService corSvc;
        private ClientSplitProviderService cspSvc;
        public bool IsStarted { get; private set; }
        public string EntryURL { get; set; }

        public void Init(string entryURL) {
            if (IsStarted)
                return;

            EntryURL = entryURL;
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;

            IDictionary props = new Hashtable();
            props["port"] = CLIENT_CHANNEL_PORT;

            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            corSvc = new ClientOutputReceiverService(
                string.Format("tcp://localhost:{0}/{1}", CLIENT_CHANNEL_PORT, CLIENT_OUTPUTRECV_SVCNAME));
            cspSvc = new ClientSplitProviderService(
                string.Format("tcp://localhost:{0}/{1}", CLIENT_CHANNEL_PORT, CLIENT_SPLITPROV_SVCNAME));

            IsStarted = true;
            RemotingServices.Marshal(corSvc, CLIENT_OUTPUTRECV_SVCNAME, typeof(ClientOutputReceiverService));
            RemotingServices.Marshal(cspSvc, CLIENT_SPLITPROV_SVCNAME, typeof(ClientSplitProviderService));

            Debug.WriteLine("Client Output Receiver Service, available at {0}.", corSvc.ServiceURL);
            Debug.WriteLine("Client Split Provider Service, available at {0}.", cspSvc.ServiceURL);
        }

        public void Submit(string filePath, int nSplits, string outputDir, string mapClassName, string assemblyFilePath) {
            this.cspSvc.SplitAndSave(filePath, nSplits);

            byte[] code = File.ReadAllBytes(assemblyFilePath);
            //TODO: submit job to worker at EntryURL
        }

        public string GetSplitProviderServiceURL() {
            return cspSvc.ServiceURL;
        }

        public string GetOutputReceiverServiceURL() {
            return corSvc.ServiceURL;
        }
    }
}