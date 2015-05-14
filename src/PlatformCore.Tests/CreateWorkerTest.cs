using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PlatformCore.Tests
{
    [TestClass]
    public class CreateWorkerTest
    {
        [TestMethod]
        public void TestCreateWorker() {
            var pm = new PuppetMasterService();
            pm.CreateWorker(1, @"tcp://localhost:30001/W", null);
            pm.GetStatus();
        }
    }
}