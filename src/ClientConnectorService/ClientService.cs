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
		private const int RESULT_WAIT_TIMEOUT = 5000;
		public const string CLIENT_OUTPUTRECV_SVCNAME = "MNRP-ClientORS";
		public const string CLIENT_SPLITPROV_SVCNAME = "MNRP-ClientSPS";
		private Guid clientId = Guid.NewGuid();

		public string EntryUrl { get; set; }

		public static Uri ClientOutputServiceUri {
			get { return new Uri(string.Format("tcp://{2}:{0}/{1}", CLIENT_CHANNEL_PORT, CLIENT_OUTPUTRECV_SVCNAME, Util.GetHostIpAddress())); }
		}

		public static Uri ClientSplitProviderServiceUri {
			get { return new Uri(string.Format("tcp://{2}:{0}/{1}", CLIENT_CHANNEL_PORT, CLIENT_SPLITPROV_SVCNAME, Util.GetHostIpAddress())); }
		}

		/// <summary>
		/// TODO: comment me.
		/// </summary>
		/// <param name="entryUrl"></param>
		public void Init(string entryUrl) {
			EntryUrl = entryUrl;
			var provider = new BinaryServerFormatterSinkProvider {
				TypeFilterLevel = TypeFilterLevel.Full
			};

			IDictionary props = new Hashtable();
			props["port"] = CLIENT_CHANNEL_PORT;

			try {
				var channel = new TcpChannel(props, null, provider);
				ChannelServices.RegisterChannel(channel, false);
			} catch {
				Trace.WriteLine("Client channel already registered, skipping this step!");
			}

			RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientOutputReceiverService), CLIENT_OUTPUTRECV_SVCNAME, WellKnownObjectMode.Singleton);
			RemotingConfiguration.RegisterWellKnownServiceType(typeof(ClientSplitProviderService), CLIENT_SPLITPROV_SVCNAME, WellKnownObjectMode.Singleton);
			//RemotingServices.Marshal(new ClientOutputReceiverService(), CLIENT_OUTPUTRECV_SVCNAME, typeof(ClientOutputReceiverService));
			//RemotingServices.Marshal(new ClientSplitProviderService(), CLIENT_SPLITPROV_SVCNAME, typeof(ClientSplitProviderService));

			Trace.WriteLine("Client Output Receiver Service, available at {0}.", ClientOutputServiceUri.ToString());
			Trace.WriteLine("Client Split Provider Service, available at {0}.", ClientSplitProviderServiceUri.ToString());
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
			var corSvc = RemotingHelper.GetRemoteObject<ClientOutputReceiverService>(ClientOutputServiceUri);
			var cspSvc = RemotingHelper.GetRemoteObject<ClientSplitProviderService>(ClientSplitProviderServiceUri);

			// Calls the server reference of the Split Provider to split and save of the file splits
			// to a memory store.
			cspSvc.SplitAndSave(filePath, nSplits, clientId);
			var masterWorker = RemotingHelper.GetRemoteObject<IWorker>(EntryUrl);

			// Creates a job to submit to the master worker.
			IJobTask job = new JobTask() {
				FileName = filePath,
				MapClassName = mapClassName,
				MapFunctionAssembly = File.ReadAllBytes(assemblyFilePath),
				SplitProviderUrl = ClientSplitProviderServiceUri.ToString(),
				OutputReceiverUrl = ClientOutputServiceUri.ToString(),
				SplitNumber = -1,
				FileSplits = new List<int>()
			};

			// Creates a list of splits.
			for (var i = 0; i < nSplits; job.FileSplits.Add(++i))
				;

			// Calls the non blocking function to send the job to the master worker.
			Trace.WriteLine("Job '" + filePath + "' setup is ready, sending it to master.");
			masterWorker.ReceiveMapJob(job);

			// Blocks the current thread until the result is ready.
			//var waitCount = 0;
			while (!corSvc.IsMapResultReady(filePath, nSplits)) {
				Trace.WriteLine(string.Format("Waiting for 'corSvc.IsMapResultReady' for file '{0}' with '{1}' splits.", filePath, nSplits));
				Thread.Sleep(RESULT_WAIT_TIMEOUT);
			}

			// Saves the map job output to disk.
			var result = corSvc.GetMapResult(filePath);

			Trace.WriteLine("Result received, rows returned: '" + (result != null ? result.Count : 0) + "'.");
			Trace.WriteLine("Sending output to '" + outputDir + "'.");

			if (result == null) {
				Trace.WriteLine("Map result received is NULL, ups we have a problem!");
				return;
			}

            for (int i = 0; i < result.Count; i++)
            {
                var outFilePath = Path.Combine(outputDir, (i + 1) + ".out");

                if (File.Exists(outFilePath))
                    File.Delete(outFilePath);

                using (var outFile = File.CreateText(outFilePath))
                {
                    outFile.WriteLine(result[i]);
                }
            }
            Trace.WriteLine("Result committed to out files. All done!");
		}
	}
}