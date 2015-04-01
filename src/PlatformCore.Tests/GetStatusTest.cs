using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PlatformCore.Tests
{
    [TestClass]
    public class GetStatusTest
    {
        PuppetMasterService puppetMaster = new PuppetMasterService();
        String remoteServiceUrl = "tcp://localhost:9000/worker";
        String remoteServiceUrl1 = "tcp://localhost:9001/worker1";
        String remoteServiceUrl2 = "tcp://localhost:9002/worker2";
        [TestMethod]
        public void TestGetStatus()
        {
            puppetMaster.CreateWorker(0, remoteServiceUrl, null);
            puppetMaster.CreateWorker(1, remoteServiceUrl1, null);
            puppetMaster.CreateWorker(2, remoteServiceUrl2, null);
            puppetMaster.GetStatus();

        }
    }
}
