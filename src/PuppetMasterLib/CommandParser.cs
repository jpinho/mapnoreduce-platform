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
        private string regexStripComments = "(^(%[^\n]*\n?))|(\n[ \t]*%[^\n]*)|(\n(?=\n))";

        public delegate void createWorker(int workerId, string puppetMasterUrl, string serviceUrl, string entryUrl);
        public delegate void submitJob(string entryUrl, string filePath, string outputPath, int splits, string mapFunctionPath);
        public delegate void wait(int secs);
        public delegate string getStatus();
        public delegate void slowWorker(int workerId);
        public delegate void freezeWorker(int workerId);
        public delegate void unfreezeWorker(int workerId);
        public delegate void freezeJobTracker(int workerId);
        public delegate void unfreezeJobTracker(int workerId);

        public createWorker CreateWorker { get; set; }
        public submitJob SubmitJob { get; set; }
        public wait Wait { get; set; }
        public getStatus GetStatus { get; set; }
        public slowWorker SlowWorker { get; set; }
        public freezeWorker FreezeWorker { get; set; }
        public unfreezeWorker UnfreezeWorker { get; set; }
        public freezeJobTracker FreezeJobTracker { get; set; }
        public unfreezeJobTracker UnfreezeJobTracker { get; set; }


        string COMMAND_TYPE_EXCEPTION = "The {0} command {1} parameter received is invalid, {2} type expected.";

        public void execute(string script) {
            Regex regex = new Regex(regexStripComments);
            String cleanScript = regex.Replace(script, "");

            string[] commands = cleanScript.Split('\n');

            foreach (string cmd in commands) {
                
                string[] keyWords = cmd.Split(' ');
                int workerId;

                switch (keyWords[0].ToLower()) { 
                    case "worker":
                        try
                        {
                            workerId = int.Parse(keyWords[1]);
                        }
                        catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        new Thread(
                            new ThreadStart(delegate(){
                                CreateWorker(workerId, keyWords[2] /*PuppetMasterUrl*/, keyWords[3] /*ServiceUrl*/, keyWords[4] /*EntryUrl*/);
                            })).Start();
                        break;
                    case "submit":
                        int splits;
                        try
                        {
                            splits = int.Parse(keyWords[4]);
                        }
                        catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[4], "Integer"), e);
                        }
                        new Thread(
                            new ThreadStart(delegate(){
                                SubmitJob(keyWords[1] /*EntryUrl*/, keyWords[2] /*FilePath*/, keyWords[3] /*OutputPath*/, splits , keyWords[5] /*MapFunctionPath*/);
                            })).Start();
                        break;
                    case "wait":
                        int secs;
                        try
                        {
                            secs = int.Parse(keyWords[1]);
                        }
                        catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        new Thread(
                            new ThreadStart(delegate(){
                                Wait(secs);
                            })).Start();
                        break;
                    case "status":
                        new Thread(
                            new ThreadStart(delegate(){
                                GetStatus();
                            })).Start();
                        break;
                    case "sloww":
                        try
                        {
                            workerId = int.Parse(keyWords[1]);
                        }
                        catch (Exception e) {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        new Thread(
                            new ThreadStart(delegate(){
                                SlowWorker(workerId);
                            })).Start();
                        break;
                    case "freezew":
                        try
                        {
                            workerId = int.Parse(keyWords[1]);
                        }
                        catch (Exception e)
                        {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        new Thread(
                            new ThreadStart(delegate(){
                                FreezeWorker(workerId);
                            })).Start();
                        break;
                    case "unfreezew":
                        try
                        {
                            workerId = int.Parse(keyWords[1]);
                        }
                        catch (Exception e)
                        {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        new Thread(
                            new ThreadStart(delegate(){
                                UnfreezeWorker(workerId);
                            })).Start();
                        break;
                    case "freezec":
                        try
                        {
                            workerId = int.Parse(keyWords[1]);
                        }
                        catch (Exception e)
                        {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        new Thread(
                            new ThreadStart(delegate(){
                                FreezeJobTracker(workerId);
                            })).Start();
                        break;
                    case "unfreezec":
                        try
                        {
                            workerId = int.Parse(keyWords[1]);
                        }
                        catch (Exception e)
                        {
                            throw new CommandInvalidParameterException(string.Format(COMMAND_TYPE_EXCEPTION, keyWords[0], keyWords[1], "Integer"), e);
                        }
                        new Thread(
                            new ThreadStart(delegate(){
                                UnfreezeJobTracker(workerId);
                            })).Start();
                        break;
                    default:
                        /*throw new UnrecognizedCommandException(cmd);*/
                        break;
                
                }
            }          
        }
    }
}
