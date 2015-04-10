using System;
using System.IO;
using ClientServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedTypes;

namespace PlatformCore.Tests
{
    [TestClass]
    public class WorkerTest
    {
        private PuppetMasterService puppetMaster = new PuppetMasterService();
        private String worker1ServiceUrl = "tcp://localhost:9000/worker1";
        private String worker2ServiceUrl = "tcp://localhost:9001/worker2";
        private String worker3ServiceUrl = "tcp://localhost:9002/worker3";
        private String worker4ServiceUrl = "tcp://localhost:9003/worker4";
        private String worker5ServiceUrl = "tcp://localhost:9004/worker5";

        [TestMethod]
        public void TestWorker() {
            // Creates the client services to provide splits data and receive split results.
            var clientService = new ClientService();
            clientService.Init(worker1ServiceUrl);

            // Creates a worker at the given URL.
            puppetMaster.CreateWorker(1, worker1ServiceUrl, null);
            puppetMaster.CreateWorker(2, worker2ServiceUrl, null);
            puppetMaster.CreateWorker(3, worker3ServiceUrl, null);
            puppetMaster.CreateWorker(4, worker4ServiceUrl, null);
            puppetMaster.CreateWorker(5, worker5ServiceUrl, null);

            // Fetchs the object from 'server'.
            var remoteWorker1 = RemotingHelper.GetRemoteObject<IWorker>(worker1ServiceUrl);
            var jobFilePath = Path.Combine(Environment.CurrentDirectory, "Resources", "job.txt");
            var jobOutputPath = string.Concat(jobFilePath, "." + DateTime.Now.ToString("ddMMyyHHmmssfff") + ".out");
            var asmPath = Path.Combine(Environment.CurrentDirectory, "Resources", "UserMappersLib.dll");

            clientService.Submit(jobFilePath, 5, jobOutputPath, "MonkeyMapper", asmPath);

            //TODO: finish result verification (read results from the output folder)!
        }

        [TestMethod]
        public void TestJobTrackerStart() {
            // creates the worker and gets remote reference to it.
            puppetMaster.CreateWorker(1, worker1ServiceUrl, null);
            var remoteWorker1 = RemotingHelper.GetRemoteObject<IWorker>(worker1ServiceUrl);

            var tracker = new JobTracker((Worker)remoteWorker1, new JobTask());
            tracker.Start(JobTrackerStatus.Active);
        }
    }
}