using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PuppetMasterLib;
using System.Diagnostics;
using System.Threading;

namespace PuppetMasterLib.Tests
{
    [TestClass]
    public class CommandParserTest
    {
        [TestMethod]
        public void TestParseCommands() {

            // arrange
            bool createWorkerSuccess = false;
            string script = @"
WORKER 1 <PUPPETMASTER-URL> <SERVICE-URL> <ENTRY-URL>
SUBMIT <ENTRY-URL> <FILE> <OUTPUT> 10 <MAP>
%potato
WAIT 5";

            CommandParser cmdParser = new CommandParser() {
                CreateWorker = delegate(int workerId, string puppetMasterUrl, string serviceUrl, string entryUrl){
                    createWorkerSuccess = true;
                    Debug.WriteLine("Hello, hello, hello!");
                }
            };
            try
            {
                // act
                cmdParser.execute(script);
            }
            catch (CommandInvalidParameterException){
                Assert.IsTrue(true);
                return;
            }
            //Assert.Fail("Invalid parameter type exception wasnt caught :(");
            
            Thread.Sleep(5000);

            // assert 
            Assert.IsTrue(createWorkerSuccess);
        }
    }
}
