using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClientServices;

namespace ClientServices.Tests
{
    [TestClass]
    public class ClientSplitProviderServiceTest
    {
        string entryURL = "TCP://LOCALHOST:9009/WORKERTEST";
        string filePath = @".\job.txt";
        int splits = 2;
        static ClientService client = new ClientService();

        [TestMethod]
        public void TestSplitAndSave() {
                    
                     
            try
            {
                // arrange

                client.Init(entryURL);
 
                // act

                ClientSplitProviderService cspSvc = (ClientSplitProviderService)Activator.GetObject(
                typeof(ClientSplitProviderService),
                client.GetSplitProviderServiceURL());

                cspSvc.SplitAndSave(filePath, splits);
                
            }
            catch (Exception e)
            {
                Assert.Fail("Something went wrong: "+ e);
            }

            // assert 
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestGetFileSplit() {
            string split = null;

            try
            {
                // arrange

                client.Init(entryURL);

                // act

                ClientSplitProviderService cspSvc = (ClientSplitProviderService)Activator.GetObject(
                typeof(ClientSplitProviderService),
                client.GetSplitProviderServiceURL());

                split = cspSvc.GetFileSplit(filePath, 1);

            }
            catch (Exception e)
            {
                Assert.Fail("Something went wrong: " + e);
            }
            

            // assert 
            Assert.IsTrue(split.Length > 0);
        }
    }
}
