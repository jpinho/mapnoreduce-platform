using System;
using SharedTypes;

namespace PuppetMasterLib.Commands
{
	public class UnfreezeWorker : ICommand
	{
		public const string NAME = "unfreezew";
		public int WorkerId { get; set; }
		public Uri ServiceUri { get; set; }

		public void Execute() {
			if (ServiceUri == null)
				ServiceUri = Globals.LocalPuppetMasterUri;
			var pMaster = (IPuppetMasterService)Activator.GetObject(
				typeof(IPuppetMasterService),
				ServiceUri.ToString());
			pMaster.UnfreezeWorker(WorkerId);
		}

		public override string ToString() {
			return NAME;
		}
	}
}