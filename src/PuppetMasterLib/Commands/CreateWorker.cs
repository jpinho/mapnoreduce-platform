using System;
using System.Text.RegularExpressions;
using SharedTypes;

namespace PuppetMasterLib.Commands
{
    public class CreateWorker : ICommand
    {
        public const string NAME = "worker";

        public int WorkerId { get; set; }

        public string PuppetMasterUrl { get; set; }

        public string ServiceUrl { get; set; }

        public string EntryUrl { get; set; }

        public void Execute() {
            var pMaster = (IPuppetMasterService)Activator.GetObject(
                typeof(IPuppetMasterService),
                PuppetMasterUrl.Trim());
            pMaster.CreateWorker(WorkerId
                , new Regex(Util.LOCALHOST_REGEX, RegexOptions.IgnoreCase).Replace(ServiceUrl.Trim(), Util.GetHostIpAddress())
                , EntryUrl);
        }

        public override string ToString() {
            return NAME;
        }
    }
}