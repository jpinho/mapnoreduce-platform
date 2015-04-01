using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedTypes;

namespace PuppetMasterLib.Commands
{
    public class Status : ICommand
    {
        public const string NAME = "status";

        public void Execute() {
            /*contact every puppetMasters?*/
            
            /* contact puppetMaster at PuppetMasterURL */
            IPuppetMasterService pMaster = (IPuppetMasterService)Activator.GetObject(
                typeof(IPuppetMasterService),
                "tcp://localhost:9008/MNRP-PuppetMasterService");
            //TODO fix hardcoded puppetmasterurl
            
            /* asks him for status*/
            pMaster.GetStatus();
        }

        public override string ToString() {
            return NAME;
        }
    }
}
