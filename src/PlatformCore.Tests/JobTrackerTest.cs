using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlatformCore;
using SharedTypes;


namespace PlatformCore.Tests
{
    [TestClass]
    public class JobTrackerTest
    {
        PuppetMasterService puppetMaster = new PuppetMasterService();
        String remoteServiceUrl = "tcp://localhost:9000/worker";
        [TestMethod]
        public void TestMethod1()
        {
            puppetMaster.CreateWorker(1, remoteServiceUrl,null);
            var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(
       remoteServiceUrl);

        }
    }
}
