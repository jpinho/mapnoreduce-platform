using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using PlatformCore;
using SharedTypes;

namespace ClientServices
{
    public class ClientService : MarshalByRefObject, IClientService
    {
        public const int CLIENT_CHANNEL_PORT = 8090;
        public const string CLIENT_OUTPUTRECV_SVCNAME = "MNRP-ClientORS";
        public const string CLIENT_SPLITPROV_SVCNAME = "MNRP-ClientSPS";
        private const int RESULT_WAIT_TIMEOUT = 5000;
        private const int RESULT_WAIT_LIMIT = 6;

        private ClientOutputReceiverService corSvc;
        private ClientSplitProviderService cspSvc;
        public string EntryUrl { get; set; }
        public bool IsStarted { get; private set; }

        /// <summary>
        /// TODO: comment me.
        /// </summary>
        /// <returns></returns>
        public string GetOutputReceiverServiceUrl() {
            return corSvc.ServiceURL;
        }

        /// <summary>
        /// TODO: comment me.
        /// </summary>
        /// <returns></returns>
        public string GetSplitProviderServiceUrl() {
            return cspSvc.ServiceURL;
        }

        /// <summary>
        /// TODO: comment me.
        /// </summary>
        /// <param name="entryUrl"></param>
        public void Init(string entryUrl) {
            if (IsStarted)
                return;

            EntryUrl = entryUrl;
            var provider = new BinaryServerFormatterSinkProvider {
                TypeFilterLevel = TypeFilterLevel.Full
            };

            IDictionary props = new Hashtable();
            props["port"] = CLIENT_CHANNEL_PORT;

            var channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            corSvc = new ClientOutputReceiverService(
                string.Format("tcp://localhost:{0}/{1}", CLIENT_CHANNEL_PORT, CLIENT_OUTPUTRECV_SVCNAME));
            cspSvc = new ClientSplitProviderService(
                string.Format("tcp://localhost:{0}/{1}", CLIENT_CHANNEL_PORT, CLIENT_SPLITPROV_SVCNAME));

            IsStarted = true;
            RemotingServices.Marshal(corSvc, CLIENT_OUTPUTRECV_SVCNAME, typeof(ClientOutputReceiverService));
            RemotingServices.Marshal(cspSvc, CLIENT_SPLITPROV_SVCNAME, typeof(ClientSplitProviderService));

            Trace.WriteLine("Client Output Receiver Service, available at {0}.", corSvc.ServiceURL);
            Trace.WriteLine("Client Split Provider Service, available at {0}.", cspSvc.ServiceURL);
        }

        /// <summary>
        /// Submits the given file at the specified file path, to the worker available at the <see
        /// cref="EntryUrl"/>. The file splits S is processed by M workers, where typically |S|
        /// &gt;&gt; |M|. At the end the result is written to the output directory specified.
        /// </summary>
        /// <param name="filePath">The path to the file being processed.</param>
        /// <param name="nSplits">The number of the splits that the file should be splitted into.</param>
        /// <param name="outputDir">The output directory where the result will be available.</param>
        /// <param name="mapClassName">The name of the Map Class.</param>
        /// <param name="assemblyFilePath">The file path to the assembly containning the map functions.</param>
        public void Submit(string filePath, int nSplits, string outputDir, string mapClassName, string assemblyFilePath) {
            // Calls the server reference of the Split Provider to split and save of the file splits
            // to a memory store.
            cspSvc.SplitAndSave(filePath, nSplits);
            var masterWorker = RemotingHelper.GetRemoteObject<IWorker>(EntryUrl);

            // Creates a job to submit to the master worker.
            IJobTask job = new JobTask() {
                FileName = filePath,
                MapClassName = mapClassName,
                MapFunctionAssembly = File.ReadAllBytes(assemblyFilePath),
                SplitProviderUrl = cspSvc.ServiceURL,
                OutputReceiverUrl = corSvc.ServiceURL,
                SplitNumber = -1,
                FileSplits = new List<int>()
            };

            // Creates a list of splits.
            for (var i = 0; i < nSplits; job.FileSplits.Add(++i))
                ;

            // Calls the non blocking function to send the job to the master worker.
            masterWorker.ReceiveMapJob(job);

            // Blocks the current thread until the result is ready.
            var waitCount = 0;
            while (!corSvc.IsMapResultReady(filePath, nSplits)) {
                if (waitCount++ >= RESULT_WAIT_LIMIT)
                    throw new TimeoutException("ClientOutputResult service 'IsMapResultReady' wait timed out!");
                Thread.Sleep(RESULT_WAIT_TIMEOUT*3);
            }

            // Saves the map job output to disk.
            var result = corSvc.GetMapResult(filePath);
            var outFilePath = Path.Combine(outputDir, filePath + ".out");

            if (File.Exists(outFilePath))
                File.Delete(outFilePath);

            using (var outFile = File.CreateText(outFilePath)) {
                foreach (var splitResult in result)
                    outFile.WriteLine(String.Join("\n", splitResult));
            }
        }
    }
}