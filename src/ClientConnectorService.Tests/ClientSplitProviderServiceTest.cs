using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientServices.Tests
{
	[TestClass]
	public class ClientSplitProviderServiceTest
	{
		private string entryUrl = "TCP://LOCALHOST:9009/WORKERTEST";
		private string filePath = Path.Combine(Environment.CurrentDirectory, "Resources\\job.txt");
		private int splits = 2;
		private static ClientService _client = new ClientService();
		private Guid clientId = Guid.NewGuid();

		[TestMethod]
		public void TestSplitAndSave() {

			try {
				// arrange
				_client.Init(entryUrl);

				// act
				var cspSvc = (ClientSplitProviderService)Activator.GetObject(
				typeof(ClientSplitProviderService),
				ClientService.ClientSplitProviderServiceUri.ToString());

				cspSvc.SplitAndSave(filePath, splits, clientId);

			} catch (Exception e) {
				Assert.Fail("Something went wrong: " + e);
			}

			// assert
			Assert.IsTrue(true);
		}

		[TestMethod]
		public void TestGetFileSplit() {
			string split = null;

			try {
				// arrange
				_client.Init(entryUrl);

				// act
				var cspSvc = (ClientSplitProviderService)Activator.GetObject(
					typeof(ClientSplitProviderService),
					ClientService.ClientSplitProviderServiceUri.ToString());

				split = cspSvc.GetFileSplit(filePath, 1);
			} catch (Exception e) {
				Assert.Fail("Something went wrong: " + e);
			}

			// assert
			Assert.IsTrue(split.Length > 0);
		}
	}
}