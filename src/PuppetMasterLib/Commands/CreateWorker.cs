using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedTypes;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;

namespace PuppetMasterLib.Commands
{
    public class CreateWorker : ICommand
    {
        public const string NAME = "worker";

        public int WorkerId { get; set; }
        public string PuppetMasterURL { get; set; }
        public string ServiceURL { get; set; }
        public string EntryURL { get; set; } /*optional*/

        public void Execute() {
          
            /*contact puppetMaster at PuppetMasterURL*/

            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);

            IPuppetMasterService pMaster = (IPuppetMasterService)Activator.GetObject(
                typeof(IPuppetMasterService),
                PuppetMasterURL);

            /*asks him to create a worker with the given WorkerId 
             * and expose its service at ServiceURL*/
            pMaster.createWorker();

            /*if EntryURL 
             *      notify existing workers 
             *      -----
             *      that it has started by calling 
             *      the worker listening at EntryURL  
             *      ----
             *      wait.. what?!*/


            //TODO: Implement me.
        }

        public override string ToString() {
            return NAME;
        }
    }
}
