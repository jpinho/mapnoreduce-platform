using System.Threading;
using SharedTypes;

namespace PuppetMasterLib.Commands
{
	public class Wait : ICommand
	{
		public const string NAME = "wait";
		public int Secs { get; set; }

		public void Execute() {
			Thread.Sleep(Secs);
		}

		public override string ToString() {
			return NAME;
		}
	}
}