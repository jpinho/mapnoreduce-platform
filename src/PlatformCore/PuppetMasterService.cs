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
				worker.GetStatus();
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

			worker.Slow(seconds);
		}

		public void FreezeWorker(int workerId) {
			IWorker worker;

			try {
				worker = workers[workerId];
			} catch (Exception e) {
				throw new InvalidWorkerIdException(workerId, e);
			}

			worker.Freeze();
		}

		public void UnfreezeWorker(int workerId) {
			IWorker worker;

			try {
				worker = workers[workerId];
			} catch (Exception e) {
				throw new InvalidWorkerIdException(workerId, e);
			}

			worker.UnFreeze();
		}

		public void FreezeCommunication(int workerId) {
			IWorker worker;

			try {
				worker = workers[workerId];
			} catch (Exception e) {
				throw new InvalidWorkerIdException(workerId, e);
			}

			worker.FreezeCommunication();
		}

		public void UnfreezeCommunication(int workerId) {
			IWorker worker;

			try {
				worker = workers[workerId];
			} catch (Exception e) {
				throw new InvalidWorkerIdException(workerId, e);
			}

			worker.UnfreezeCommunication();
		}

		private void NotifyWorkerCreation(IWorker worker) {
			Trace.WriteLine("Sends notification to worker at ENTRY_URL informing worker creation.");
			//TODO: Contact worker at ENTRY_URL and announce new worker available.
		}

		/// <summary>
		/// Serves a Marshalled Puppet Master object at a specific IChannel under ChannelServices.
		/// </summary>
		public static void Run() {
			var service = new PuppetMasterService();
			RemotingHelper.CreateService(service, ServiceUrl, true);
			Trace.WriteLine("Puppet Master Service listening at '" + ServiceUrl + "'");
		}
	}
}