using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedTypes;

namespace PuppetMasterLib.Commands
{
    public class SlowWorker : ICommand
    {
        public const string NAME = "sloww";
        public int WorkerId { get; set; }
        public int Secs { get; set; }

        public void Execute() {
            /*contact every puppetMasters?*/

            /* contact puppetMaster at PuppetMasterURL */
            IPuppetMasterService pMaster = (IPuppetMasterService)Activator.GetObject(
                typeof(IPuppetMasterService),
                "tcp://localhost:9008/MNRP-PuppetMasterService");
            //TODO fix hardcoded puppetmasterurl

            /* asks him to slow worker some seconds*/
            pMaster.SlowWorker(WorkerId, Secs);
        }

        public override string ToString() {
            return NAME;
        }
    }
}

