﻿using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientServices.Tests
{
    [TestClass]
    public class ClientSplitProviderServiceTest
    {
        private string entryURL = "TCP://LOCALHOST:9009/WORKERTEST";
        private string filePath = Path.Combine(Environment.CurrentDirectory, "Resources\\job.txt");
        private int splits = 2;
        private static ClientService client = new ClientService();

        [TestMethod]
        public void TestSplitAndSave() {

            try {
                // arrange
                client.Init(entryURL);

                // act
                ClientSplitProviderService cspSvc = (ClientSplitProviderService)Activator.GetObject(
                typeof(ClientSplitProviderService),
                client.GetSplitProviderServiceUrl());

                cspSvc.SplitAndSave(filePath, splits);

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
                client.Init(entryURL);

                // act
                ClientSplitProviderService cspSvc = (ClientSplitProviderService)Activator.GetObject(
                    typeof(ClientSplitProviderService),
                    client.GetSplitProviderServiceUrl());

                split = cspSvc.GetFileSplit(filePath, 1);
            } catch (Exception e) {
                Assert.Fail("Something went wrong: " + e);
            }

            // assert
            Assert.IsTrue(split.Length > 0);
        }
    }
}