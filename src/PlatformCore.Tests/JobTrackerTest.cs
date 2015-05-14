using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ClientServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedTypes;

namespace PlatformCore.Tests
{

    [TestClass]
    public class JobTrackerTest
    {
        private readonly PuppetMasterService puppetMaster = new PuppetMasterService();
        private readonly string worker1ServiceUrl = "tcp://localhost:20000/worker" + Guid.NewGuid().ToString("D");
        //private readonly string worker2ServiceUrl = "tcp://localhost:20001/worker" + Guid.NewGuid().ToString("D");
        //private readonly string worker3ServiceUrl = "tcp://localhost:20002/worker" + Guid.NewGuid().ToString("D");
        //private readonly string worker4ServiceUrl = "tcp://localhost:20003/worker" + Guid.NewGuid().ToString("D");
        //private readonly string worker5ServiceUrl = "tcp://localhost:20004/worker" + Guid.NewGuid().ToString("D");

        [TestMethod]
        public void TestJobTrackerFreeze() {
            // Creates the client services to provide splits data and receive split results.
            var clientService = new ClientService();
            clientService.Init(worker1ServiceUrl);

            // Creates a worker at the given URL.
            puppetMaster.CreateWorker(1, worker1ServiceUrl, null);

            // Fetchs the object from 'server'.
            var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker1ServiceUrl);
            var jobFilePath = Path.Combine(Environment.CurrentDirectory, "Resources", "job.txt");
            var jobOutputPath = string.Concat(jobFilePath, "." + DateTime.Now.ToString("ddMMyyHHmmssfff") + ".out");
            var asmPath = Path.Combine(Environment.CurrentDirectory, "Resources", "UserMappersLib.dll");

            new Thread(() => clientService.Submit(jobFilePath, 5, jobOutputPath, "MonkeyMapper", asmPath)).Start();

            //giving time to initiate trackers
            Thread.Sleep(2000);

            Trace.WriteLine("Freeze tracker");
            remoteWorker.FreezeCommunication();

            Thread.Sleep(10 * 1000);

            Trace.WriteLine("Unfreeze tracker");
            remoteWorker.UnfreezeCommunication();

            Thread.Sleep(1000);
            Trace.WriteLine("The end");

        }
    }
}