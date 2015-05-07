using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SharedTypes;

namespace PlatformCore
{
	[Serializable]
	public class PuppetMasterService : MarshalByRefObject, IPuppetMasterService
	{
		private readonly object globalLock = new object();
		private readonly Dictionary<int, IWorker> workers = new Dictionary<int, IWorker>();
		public static readonly string ServiceName = "MNRP-PuppetMasterService";
		public static readonly Uri ServiceUrl = Globals.LocalPuppetMasterUri;

		public PuppetMasterService() {
			// Required for .NET Remoting Proxy Classes.
		}

		public Uri GetServiceUri() {
			return ServiceUrl;
		}

		public void CreateWorker(int workerId, string serviceUrl, string entryUrl) {
			lock (globalLock) {
				var serviceUri = new Uri(serviceUrl);
				RemotingHelper.RegisterChannel(serviceUri);

				var remoteWorker = Worker.Run(workerId, serviceUri, workers);
				workers.Add(workerId, remoteWorker);
				remoteWorker.UpdateAvailableWorkers(workers);

				Trace.WriteLine(string.Format("New worker created: id '{0}', url '{1}'."
					, workerId, serviceUri));

				if (!string.IsNullOrWhiteSpace(entryUrl))
					NotifyWorkerCreation(remoteWorker);
			}
		}

		public void GetStatus() {
			foreach (var worker in workers.Values) {
				var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
				remoteWorker.GetStatus();
			}
		}

		public Dictionary<int, IWorker> GetWorkers() {
			return workers;
		}

		public void Wait(int seconds) {
			Thread.Sleep(seconds * 1000);
		}

		public void SlowWorker(int workerId, int seconds) {
			IWorker worker;

			try {
				worker = workers[workerId];
			} catch (Exception e) {
				throw new InvalidWorkerIdException(workerId, e);
			}

			var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
			remoteWorker.Slow(seconds);
		}

		public void FreezeWorker(int workerId) {
			IWorker worker;

			try {
				worker = workers[workerId];
			} catch (Exception e) {
				throw new InvalidWorkerIdException(workerId, e);
			}

			var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
			remoteWorker.Freeze();
		}

		public void UnfreezeWorker(int workerId) {
			IWorker worker;

			try {
				worker = workers[workerId];
			} catch (Exception e) {
				throw new InvalidWorkerIdException(workerId, e);
			}

			var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
			remoteWorker.UnFreeze();
		}

		public void FreezeCommunication(int workerId) {
			IWorker worker;

			try {
				worker = workers[workerId];
			} catch (Exception e) {
				throw new InvalidWorkerIdException(workerId, e);
			}

			var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
			remoteWorker.FreezeCommunication();
		}

		public void UnfreezeCommunication(int workerId) {
			IWorker worker;

			try {
				worker = workers[workerId];
			} catch (Exception e) {
				throw new InvalidWorkerIdException(workerId, e);
			}

			var remoteWorker = RemotingHelper.GetRemoteObject<IWorker>(worker.ServiceUrl);
			remoteWorker.UnfreezeCommunication();
		}

		private void NotifyWorkerCreation(IWorker worker) {
			Trace.WriteLine("Sends notification to worker at ENTRY_URL informing worker creation.");
			//TODO: Contact worker at ENTRY_URL and announce new worker available.
		}

		/// <summary>
		/// Serves a Marshalled Puppet Master object at a specific IChannel under ChannelServices.
		/// </summary>
		public static void Run() {
			RemotingHelper.RegisterChannel(ServiceUrl);
			RemotingHelper.CreateWellKnownService(typeof(PuppetMasterService), ServiceName);
			Trace.WriteLine("Puppet Master Service endpoint ready at '" + ServiceUrl + "'");
		}
	}
}