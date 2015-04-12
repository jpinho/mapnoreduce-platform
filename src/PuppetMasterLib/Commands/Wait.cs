using System;
using SharedTypes;

namespace PuppetMasterLib.Commands
{
	public class Wait : ICommand
	{
		public const string NAME = "wait";
		public int Secs { get; set; }
		public Uri ServiceUri { get; set; }

		public void Execute() {
			if (ServiceUri == null)
				ServiceUri = Globals.LocalPuppetMasterUri;

			var pMaster = (IPuppetMasterService)Activator.GetObject(
				typeof(IPuppetMasterService),
				ServiceUri.ToString());
			pMaster.Wait(Secs);
		}

		public override string ToString() {
			return NAME;
		}
	}
}