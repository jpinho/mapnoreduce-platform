using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedTypes;

namespace PuppetMasterLib.Commands
{
    public class UnfreezeCommunication : ICommand
    {
        public const string NAME = "unfreezec";
        public int WorkerId { get; set; }

        public void Execute() {
            IPuppetMasterService pMaster = (IPuppetMasterService)Activator.GetObject(
                typeof(IPuppetMasterService),
                "tcp://localhost:9008/MNRP-PuppetMasterService");
            //temp puppet master hardcoded
            pMaster.UnfreezeCommunication(WorkerId);
        }

        public override string ToString() {
            return NAME;
        }
    }
}
