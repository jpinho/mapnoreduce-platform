using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PlatformCore.Tests
{
	[TestClass]
	public class GetStatusTest
	{
		private PuppetMasterService puppetMaster = new PuppetMasterService();
		private String remoteServiceUrl1 = "tcp://localhost:30001/worker" + Guid.NewGuid().ToString("D");
		private String remoteServiceUrl2 = "tcp://localhost:30002/worker" + Guid.NewGuid().ToString("D");
		private String remoteServiceUrl3 = "tcp://localhost:30003/worker" + Guid.NewGuid().ToString("D");

		[TestMethod]
		public void TestGetStatus() {
			puppetMaster.CreateWorker(1, remoteServiceUrl1, null);
			puppetMaster.CreateWorker(2, remoteServiceUrl2, null);
			puppetMaster.CreateWorker(3, remoteServiceUrl3, null);
			puppetMaster.GetStatus();
		}
	}
}