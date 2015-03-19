﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PuppetMasterLib;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

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
SUBMIT <ENTRY-URL> <FILE> <OUTPUT> 10 <MAP>
%potato
WAIT 5";

            List<ICommand> commands;
            try {
                // act
                commands = CommandParser.Run(script);
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
