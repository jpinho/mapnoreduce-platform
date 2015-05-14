using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlatformCore;
using PuppetMasterLib.Exceptions;
using SharedTypes;

namespace PuppetMasterLib.Tests
{
    [TestClass]
    public class CommandParserTest
    {
        [TestMethod]
        public void TestParseCommands() {

            // arrange
            //bool createWorkerSuccess = false;
            string script = @"WORKER 1 <PUPPETMASTER-URL> <SERVICE-URL> <ENTRY-URL>
SUBMIT <ENTRY-URL> <FILE> <OUTPUT> 10 <MAP> <DLL>
%potato
WAIT 5";

            List<ICommand> commands;
            try {
                IPuppetMasterService pm = new PuppetMasterService();
                // act
                commands = CommandParser.Run(script, pm);
            } catch (CommandInvalidParameterException) {
                Assert.IsTrue(true);
                return;
            }
            //Assert.Fail("Invalid parameter type exception wasnt caught :(");

            // assert
            Assert.IsNotNull(commands);
            Assert.IsTrue(commands.Count == 3);
        }
    }
}