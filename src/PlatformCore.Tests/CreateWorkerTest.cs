using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedTypes;
using System.Diagnostics;

namespace PlatformCore.Tests
{
    [TestClass]
    public class CreateWorkerTest
    {
        [TestMethod]
        public void TestCreateWorker()
        {
            PuppetMasterService PM = new PuppetMasterService();
            PM.CreateWorker(1, @"tcp://localhost:30001/W", null);
            PM.GetStatus();
        }
    }
}
