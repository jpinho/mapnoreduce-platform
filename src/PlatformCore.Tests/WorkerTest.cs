using System;
using System.Collections.Generic;
using System.IO;
using ClientServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedTypes;

namespace PlatformCore.Tests
{
	[TestClass]
	public class WorkerTest
	{
		private readonly PuppetMasterService puppetMaster = new PuppetMasterService();
		private readonly String worker1ServiceUrl = "tcp://localhost:20000/worker" + Guid.NewGuid().ToString("D");
		private readonly String worker2ServiceUrl = "tcp://localhost:20001/worker" + Guid.NewGuid().ToString("D");
		private readonly String worker3ServiceUrl = "tcp://localhost:20002/worker" + Guid.NewGuid().ToString("D");
		private readonly String worker4ServiceUrl = "tcp://localhost:20003/worker" + Guid.NewGuid().ToString("D");
		private readonly String worker5ServiceUrl = "tcp://localhost:20004/worker" + Guid.NewGuid().ToString("D");

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
			var remoteWorker1 = RemotingHelper.GetRemoteObject<IWorker>(
				"tcp://localhost:21004/worker" + Guid.NewGuid().ToString("D"));

			var tracker = new JobTracker((Worker)remoteWorker1, new JobTask() {
				FileSplits = new List<int> { 1, 2, 3, 4, 5 },
				FileName = "job.txt",
				MapClassName = "MonkeyMapper"
			});

			//TODO: Implement me.
			//tracker.Start(JobTrackerStatus.Active);
		}
	}
}