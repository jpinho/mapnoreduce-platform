using PuppetMasterLib.Exceptions;
using SharedTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace PuppetMasterLib {
    public class CommandParser {
        private const string COMMAND_TYPE_EXCEPTION = "The {0} command {1} parameter received is invalid, {2} type expected.";
        private const string REGEX_STRIP_COMMENTS = "(^(%[^\n]*\n?))|(\n[ \t]*%[^\n]*)|(\n(?=\n))";

        public static List<ICommand> Run(string script) {
            return Run(script, null);
        }

        public static List<ICommand> Run(string script, params object[] context) {
            var regex = new Regex(REGEX_STRIP_COMMENTS);
            var cleanScript = regex.Replace(script, "");
            var commands = cleanScript.Split('\n');
            var parsedCommands = new List<ICommand>();
            var puppetMaster = context.OfType<IPuppetMasterService>().First();

            foreach (var keyWords in commands.Select(cmd => cmd.Split(' '))) {
                int workerId;

                switch (keyWords[0].Trim().ToLower()) {
                    case Commands.CreateWorker.NAME:
                        try {
                            workerId = int.Parse(keyWords[1].Trim());
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(
                                string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }

                        switch (keyWords.Length) {
                            case 5:
                                parsedCommands.Add(new Commands.CreateWorker() {
                                    WorkerId = workerId,
                                    PuppetMasterUrl = keyWords[2].Trim(),
                                    ServiceUrl = keyWords[3].Trim(),
                                    EntryUrl = keyWords[4].Trim()
                                });
                                break;
                            case 4:
                                parsedCommands.Add(new Commands.CreateWorker() {
                                    WorkerId = workerId,
                                    PuppetMasterUrl = keyWords[2].Trim(),
                                    ServiceUrl = keyWords[3].Trim()
                                });
                                break;
                        }

                        break;

                    case Commands.SubmitJob.NAME:
                        int splits;
                        try {
                            splits = int.Parse(keyWords[4].Trim());
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[4], "Integer"), e);
                        }
                        parsedCommands.Add(new Commands.SubmitJob() {
                            EntryUrl = keyWords[1].Trim(),
                            FilePath = keyWords[2].Trim(),
                            OutputPath = keyWords[3].Trim(),
                            Splits = splits,
                            MapClassName = keyWords[5].Trim(),
                            AssemblyFilePath = keyWords[6].Trim()
                        });

                        break;

                    case Commands.AnnouncePM.NAME:
                        try {
                            String pmUri = keyWords[1].Trim();
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[4], "Integer"), e);
                        }
                        parsedCommands.Add(new Commands.AnnouncePM() {
                            PuppetMasterUrl = keyWords[1].Trim()
                        });

                        break;
                    case Commands.Wait.NAME:
                        int secs;
                        try {
                            secs = int.Parse(keyWords[1].Trim());
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }

                        parsedCommands.Add(new Commands.Wait() {
                            Secs = secs,
                            ServiceUri = puppetMaster == null ? null : puppetMaster.GetServiceUri()
                        });

                        break;

                    case Commands.Status.NAME:
                        parsedCommands.Add(new Commands.Status());
                        break;

                    case Commands.SlowWorker.NAME:
                        try {
                            workerId = int.Parse(keyWords[1].Trim());
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        try {
                            secs = int.Parse(keyWords[2].Trim());
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[2], "Integer"), e);
                        }
                        parsedCommands.Add(new Commands.SlowWorker() {
                            WorkerId = workerId,
                            Secs = secs,
                            ServiceUri = puppetMaster == null ? null : puppetMaster.GetServiceUri()
                        });
                        break;

                    case Commands.FreezeWorker.NAME:
                        try {
                            workerId = int.Parse(keyWords[1].Trim());
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        parsedCommands.Add(new Commands.FreezeWorker() {
                            WorkerId = workerId,
                            ServiceUri = puppetMaster == null ? null : puppetMaster.GetServiceUri()
                        });
                        break;

                    case Commands.UnfreezeWorker.NAME:
                        try {
                            workerId = int.Parse(keyWords[1].Trim());
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        parsedCommands.Add(new Commands.UnfreezeWorker() {
                            WorkerId = workerId,
                            ServiceUri = puppetMaster == null ? null : puppetMaster.GetServiceUri()
                        });
                        break;

                    case Commands.FreezeCommunication.NAME:
                        try {
                            workerId = int.Parse(keyWords[1].Trim());
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        parsedCommands.Add(new Commands.FreezeCommunication() {
                            WorkerId = workerId,
                            ServiceUri = puppetMaster == null ? null : puppetMaster.GetServiceUri()
                        });
                        break;

                    case Commands.UnfreezeCommunication.NAME:
                        try {
                            workerId = int.Parse(keyWords[1].Trim());
                        } catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        parsedCommands.Add(new Commands.UnfreezeCommunication() {
                            WorkerId = workerId,
                            ServiceUri = puppetMaster == null ? null : puppetMaster.GetServiceUri()
                        });
                        break;
                    default:
                        Trace.TraceWarning("Ignoring unknown command '" + keyWords[0] + "'. Script received was: '"
                            + string.Join(" ", keyWords) + "'.");
                        break;
                }
            }

            return parsedCommands;
        }
    }
}