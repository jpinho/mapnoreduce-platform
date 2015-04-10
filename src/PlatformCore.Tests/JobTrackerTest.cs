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
        public void TestJobTrackerFreeze()
        {
            puppetMaster.CreateWorker(1, remoteServiceUrl,null);
            var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(remoteServiceUrl);
            //the point was freezing only jobtracker component
            remoteWorker.FreezeCommunication();

            //testing if worker is working -- yap its working
            remoteWorker.GetStatus();
            
            //unfreezing jobtracker
            remoteWorker.UnfreezeCommunication();

        }
    }
}
