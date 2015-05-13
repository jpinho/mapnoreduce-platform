using System;
using SharedTypes;

namespace PuppetMasterLib.Commands
{
	public class UnfreezeCommunication : ICommand
	{
		public const string NAME = "unfreezec";
		public int WorkerId { get; set; }
		public Uri ServiceUri { get; set; }

		public void Execute() {
			var pMaster = (IPuppetMasterService)Activator.GetObject(
				typeof(IPuppetMasterService),
				ServiceUri.ToString());
			pMaster.UnfreezeCommunication(WorkerId);
		}

		public override string ToString() {
			return NAME;
		}
	}
}