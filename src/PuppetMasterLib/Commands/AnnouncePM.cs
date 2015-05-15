using System;
using SharedTypes;

namespace PuppetMasterLib.Commands
{
    public class AnnouncePM : ICommand
    {
        public const string NAME = "announcepm";

        public int WorkerId { get; set; }

        public string PuppetMasterUrl { get; set; }

        public void Execute() {
            var pMaster = (IPuppetMasterService)Activator.GetObject(
                typeof(IPuppetMasterService),
                Globals.LocalPuppetMasterUri.ToString());
            pMaster.BroadcastAnnouncePm(new Uri(PuppetMasterUrl));
        }

        public override string ToString() {
            return NAME;
        }
    }
}