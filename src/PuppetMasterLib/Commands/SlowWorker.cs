using System;
using SharedTypes;

namespace PuppetMasterLib.Commands
{
	public class SlowWorker : ICommand
	{
		public const string NAME = "sloww";
		public int WorkerId { get; set; }
		public int Secs { get; set; }
		public Uri ServiceUri { get; set; }

		public void Execute() {
			if (ServiceUri == null)
				ServiceUri = Globals.LocalPuppetMasterUri;
			var pMaster = (IPuppetMasterService)Activator.GetObject(
				typeof(IPuppetMasterService),
				ServiceUri.ToString());
			pMaster.SlowWorker(WorkerId, Secs);
		}

		public override string ToString() {
			return NAME;
		}
	}
}