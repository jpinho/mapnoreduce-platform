using System;
using System.IO;
using ClientServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientConnectorService.Tests
{
    [TestClass]
    public class ClientSplitProviderServiceTest
    {
        private const int SPLITS = 2;
        private const string ENTRY_URL = "TCP://LOCALHOST:9009/WORKERTEST";

        private readonly string filePath = Path.Combine(Environment.CurrentDirectory, "Resources\\job.txt");
        private readonly Guid clientId = Guid.NewGuid();
        private static readonly ClientService Client = new ClientService();

        [TestMethod]
        public void TestSplitAndSave() {

            try {
                // arrange
                Client.Init(ENTRY_URL);

                // act
                var cspSvc = (ClientSplitProviderService)Activator.GetObject(
                typeof(ClientSplitProviderService),
                ClientService.ClientSplitProviderServiceUri.ToString());

                cspSvc.SplitAndSave(filePath, SPLITS, clientId);

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
                Client.Init(ENTRY_URL);

                // act
                var cspSvc = (ClientSplitProviderService)Activator.GetObject(
                typeof(ClientSplitProviderService),
                ClientService.ClientSplitProviderServiceUri.ToString());

                cspSvc.SplitAndSave(filePath, SPLITS, clientId);

                split = cspSvc.GetFileSplit(filePath, 1);

            } catch (Exception e) {
                Assert.Fail("Something went wrong: " + e);
            }

            // assert
            Assert.IsTrue(split.Length > 0);
        }
    }
}