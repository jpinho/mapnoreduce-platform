using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

namespace PuppetMasterLib
{
    public class CommandParser
    {
        const string COMMAND_TYPE_EXCEPTION = "The {0} command {1} parameter received is invalid, {2} type expected.";
        private string regexStripComments = "(^(%[^\n]*\n?))|(\n[ \t]*%[^\n]*)|(\n(?=\n))";

        public List<ICommand> run(string script) {
            Regex regex = new Regex(regexStripComments);
            String cleanScript = regex.Replace(script, "");

            string[] commands = cleanScript.Split('\n');
            List<ICommand> parsedCommands = new List<ICommand>();

            foreach (string cmd in commands) {
                string[] keyWords = cmd.Split(' ');
                int workerId;

                switch (keyWords[0].ToLower()) {
                    case Commands.CreateWorker.NAME:
                        try {
                            workerId = int.Parse(keyWords[1]);
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(
                                string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }

                        parsedCommands.Add(new Commands.CreateWorker() {
                            WorkerId = workerId,
                            PuppetMasterURL = keyWords[2],
                            ServiceURL = keyWords[3],
                            EntryURL = keyWords[4]
                        });

                        break;
                    //case Commands.SubmitJob.NAME
                    case "submit":
                        int splits;
                        try {
                            splits = int.Parse(keyWords[4]);
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[4], "Integer"), e);
                        }
                        //new Thread(
                        //    new ThreadStart(delegate() {
                        //    SubmitJob(keyWords[1] /*EntryUrl*/, keyWords[2] /*FilePath*/, keyWords[3] /*OutputPath*/, splits, keyWords[5] /*MapFunctionPath*/);
                        //})).Start();
                        break;
                    case "wait":
                        int secs;
                        try {
                            secs = int.Parse(keyWords[1]);
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        //new Thread(
                        //    new ThreadStart(delegate() {
                        //    Wait(secs);
                        //})).Start();
                        break;
                    case "status":
                        //new Thread(
                        //    new ThreadStart(delegate() {
                        //    GetStatus();
                        //})).Start();
                        break;
                    case "sloww":
                        try {
                            workerId = int.Parse(keyWords[1]);
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        //new Thread(
                        //    new ThreadStart(delegate() {
                        //    SlowWorker(workerId);
                        //})).Start();
                        break;
                    case "freezew":
                        try {
                            workerId = int.Parse(keyWords[1]);
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        //new Thread(
                        //    new ThreadStart(delegate() {
                        //    FreezeWorker(workerId);
                        //})).Start();
                        break;
                    case "unfreezew":
                        try {
                            workerId = int.Parse(keyWords[1]);
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        //new Thread(
                        //    new ThreadStart(delegate() {
                        //    UnfreezeWorker(workerId);
                        //})).Start();
                        break;
                    case "freezec":
                        try {
                            workerId = int.Parse(keyWords[1]);
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        //new Thread(
                        //    new ThreadStart(delegate() {
                        //    FreezeJobTracker(workerId);
                        //})).Start();
                        break;
                    case "unfreezec":
                        try {
                            workerId = int.Parse(keyWords[1]);
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        //new Thread(
                        //    new ThreadStart(delegate() {
                        //    UnfreezeJobTracker(workerId);
                        //})).Start();
                        break;
                    default:
                        /*throw new UnrecognizedCommandException(cmd);*/
                        break;
                }
            }

            return parsedCommands;
        }
    }
}
