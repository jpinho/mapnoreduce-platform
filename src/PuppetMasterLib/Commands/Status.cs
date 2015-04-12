using System;
using SharedTypes;

namespace PuppetMasterLib.Commands
{
	public class Status : ICommand
	{
		public const string NAME = "status";
		public Uri ServiceUri { get; set; }

		public void Execute() {
			if (ServiceUri == null)
				ServiceUri = Globals.LocalPuppetMasterUri;
			var pMaster = (IPuppetMasterService)Activator.GetObject(
				typeof(IPuppetMasterService),
				ServiceUri.ToString());
			pMaster.GetStatus();
		}

		public override string ToString() {
			return NAME;
		}
	}
}