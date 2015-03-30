using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.String;
using System.Threading.Tasks;

namespace PuppetMasterLib.Commands
{
    public class CreateWorker : ICommand
    {
        public const string NAME = "worker";

        public int WorkerId { get; set; }

        public string PuppetMasterURL { get; set; }

        public string ServiceURL { get; set; }

        public string EntryURL { get; set; }

        public void Execute() {
            /* contact puppetMaster at PuppetMasterURL */
            IPuppetMasterService pMaster = (IPuppetMasterService)Activator.GetObject(
                typeof(IPuppetMasterService),
                PuppetMasterURL);

            /* asks him to create a worker with the given WorkerId
             * and expose its service at ServiceURL */
            pMaster.CreateWorker(WorkerId, ServiceURL, EntryURL);
        }

        public override string ToString() {
            return NAME;
        }
    }
}