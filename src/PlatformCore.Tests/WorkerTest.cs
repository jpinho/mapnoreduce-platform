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
		public void TestFreezeWorker() {
			// Creates the client services to provide splits data and receive split results.
			var clientService = new ClientService();
			clientService.Init(worker1ServiceUrl);

			// Creates a worker at the given URL.
			puppetMaster.CreateWorker(1, worker1ServiceUrl, null);

			// Fetchs the object from 'server'.
			var remoteWorker1 = RemotingHelper.GetRemoteObject<IWorker>(worker1ServiceUrl);
			var jobFilePath = Path.Combine(Environment.CurrentDirectory, "Resources", "job.txt");
			var jobOutputPath = string.Concat(jobFilePath, "." + DateTime.Now.ToString("ddMMyyHHmmssfff") + ".out");
			var asmPath = Path.Combine(Environment.CurrentDirectory, "Resources", "UserMappersLib.dll");

			new Thread(() => clientService.Submit(jobFilePath, 5, jobOutputPath, "MonkeyMapper", asmPath)).Start();

			Trace.WriteLine("Lets freeze");
			/*My code works and I dont know why*/
			new Thread(() => remoteWorker1.Freeze()).Start();

			Thread.Sleep(10 * 1000);

			Trace.WriteLine("Lets Work");
			remoteWorker1.UnFreeze();

			Thread.Sleep(1000);

			/*Again*/
			Trace.WriteLine("Lets freeze again");
			/*My code works and I dont know why*/
			new Thread(() => remoteWorker1.Freeze()).Start();

			Thread.Sleep(10 * 1000);

			Trace.WriteLine("Lets Work again");
			remoteWorker1.UnFreeze();

			Thread.Sleep(10 * 1000);
		}

		[TestMethod]
		public void TestFreezeWorkerWithThread() {
			// Creates the client services to provide splits data and receive split results.
			var clientService = new ClientService();
			clientService.Init(worker1ServiceUrl);

			// Creates a worker at the given URL.
			puppetMaster.CreateWorker(1, worker1ServiceUrl, null);

			// Fetchs the object from 'server'.
			var remoteWorker1 = RemotingHelper.GetRemoteObject<IWorker>(worker1ServiceUrl);
			var jobFilePath = Path.Combine(Environment.CurrentDirectory, "Resources", "job.txt");
			var jobOutputPath = string.Concat(jobFilePath, "." + DateTime.Now.ToString("ddMMyyHHmmssfff") + ".out");
			var asmPath = Path.Combine(Environment.CurrentDirectory, "Resources", "UserMappersLib.dll");

			new Thread(() => clientService.Submit(jobFilePath, 5, jobOutputPath, "MonkeyMapper", asmPath)).Start();

			Trace.WriteLine("Lets freeze");

			remoteWorker1.Freeze();

			Thread.Sleep(10 * 1000);

			Trace.WriteLine("Lets Work");
			remoteWorker1.UnFreeze();

			Thread.Sleep(1000);

			/*Again*/
			Trace.WriteLine("Lets freeze again");

			remoteWorker1.Freeze();

			Thread.Sleep(10 * 1000);

			Trace.WriteLine("Lets Work again");
			remoteWorker1.UnFreeze();

			Thread.Sleep(10 * 1000);
		}

		[TestMethod]
		public void TestJobTrackerStart() {
			//TODO: Implement me.
		}
	}
}