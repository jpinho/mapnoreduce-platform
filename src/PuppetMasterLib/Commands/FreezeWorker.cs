using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedTypes;

namespace PuppetMasterLib.Commands
{
    public class FreezeWorker : ICommand
    {
        public const string NAME = "freezew";
        public int WorkerId { get; set; }
        public string PuppetMasterUrl { get; set; } 

        public void Execute() {
            IPuppetMasterService pMaster = (IPuppetMasterService)Activator.GetObject(
                typeof(IPuppetMasterService),
                "tcp://localhost:9008/MNRP-PuppetMasterService");
            //temp puppet master hardcoded
            pMaster.FreezeWorker(WorkerId);
        }

        public override string ToString() {
            return NAME;
        }
    }
}
