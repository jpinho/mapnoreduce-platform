using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PlatformCore.Tests
{
    [TestClass]
    public class SlowWorkerTest
    {
        [TestMethod]
        public void TestSlowWorker() {
            PuppetMasterService puppetMaster = new PuppetMasterService();
            puppetMaster.CreateWorker(1, "tcp://localhost:9010/MNRP-worker1", "tcp://localhost:9010/MNRP-worker1");
            int timeToWaitInSecs = 5;
            Stopwatch stopwatch = Stopwatch.StartNew(); //creates and start the instance of Stopwatch
            puppetMaster.SlowWorker(1, timeToWaitInSecs);
            stopwatch.Stop();
            Debug.WriteLine(stopwatch.ElapsedMilliseconds);
            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= timeToWaitInSecs * 1000);

        }
    }
}