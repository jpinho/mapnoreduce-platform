using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace PlatformCore.Tests
{
    [TestClass]
    public class WaitTest
    {
        [TestMethod]
        public void TestWait()
        {
            PuppetMasterService puppetMaster = new PuppetMasterService();
            int timeToWaitInSecs = 5;
            Stopwatch stopwatch = Stopwatch.StartNew(); //creates and start the instance of Stopwatch
            puppetMaster.Wait(timeToWaitInSecs);
            stopwatch.Stop();
            Debug.WriteLine(stopwatch.ElapsedMilliseconds);
            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= timeToWaitInSecs * 1000);

        }
    }
}
