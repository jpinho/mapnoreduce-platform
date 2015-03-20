using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using PuppetMasterLib.Exceptions;
using SharedTypes;

namespace PuppetMasterLib
{
    public class CommandParser
    {
        const string COMMAND_TYPE_EXCEPTION = "The {0} command {1} parameter received is invalid, {2} type expected.";
        const string UNRECOGNIZED_COMMAND_EXCEPTION = "The {0} command is not recognized.";
        private static string regexStripComments = "(^(%[^\n]*\n?))|(\n[ \t]*%[^\n]*)|(\n(?=\n))";

        public static List<ICommand> Run(string script) {
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
                    case Commands.SubmitJob.NAME:
                        int splits;
                        try {
                            splits = int.Parse(keyWords[4]);
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[4], "Integer"), e);
                        }
                        parsedCommands.Add(new Commands.SubmitJob() {
                            EntryURL = keyWords[1],
                            FilePath = keyWords[2],
                            OutputPath = keyWords[3],
                            Splits = splits,
                            MapFunctionPath = keyWords[5],
                            Dll = keyWords[6]

                        });
                        break;
                    case Commands.Wait.NAME:
                        int secs;
                        try {
                            secs = int.Parse(keyWords[1]);
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        parsedCommands.Add(new Commands.Wait() {
                            Secs = secs
                        });
                        break;
                    case Commands.Status.NAME:
                        parsedCommands.Add(new Commands.Status());
                        break;
                    case Commands.SlowWorker.NAME:
                        try {
                            workerId = int.Parse(keyWords[1]);
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        parsedCommands.Add(new Commands.SlowWorker() {
                            WorkerId = workerId
                        });
                        break;
                    case Commands.FreezeWorker.NAME:
                        try {
                            workerId = int.Parse(keyWords[1]);
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        parsedCommands.Add(new Commands.FreezeWorker() {
                            WorkerId = workerId
                        });
                        break;
                    case Commands.UnfreezeWorker.NAME:
                        try {
                            workerId = int.Parse(keyWords[1]);
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        parsedCommands.Add(new Commands.UnfreezeWorker() {
                            WorkerId = workerId
                        });
                        break;
                    case Commands.FreezeJobTracker.NAME:
                        try {
                            workerId = int.Parse(keyWords[1]);
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        parsedCommands.Add(new Commands.FreezeJobTracker() {
                            WorkerId = workerId
                        });
                        break;
                    case Commands.UnfreezeJobTracker.NAME:
                        try {
                            workerId = int.Parse(keyWords[1]);
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        parsedCommands.Add(new Commands.UnfreezeJobTracker() {
                            WorkerId = workerId
                        });
                        break;
                    //default:
                    //quando não percebes o comando por agora não lances excepção, ignora apenas.
                    //throw new UnrecognizedCommandException(string.Format(UNRECOGNIZED_COMMAND_EXCEPTION, cmd));
                }
            }

            return parsedCommands;
        }
    }
}
