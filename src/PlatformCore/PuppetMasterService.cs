using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharedTypes;

namespace PlatformCore
{
    public class PuppetMasterService : MarshalByRefObject, IPuppetMasterService
    {
        private readonly Dictionary<int, IWorker> workers = new Dictionary<int, IWorker>();
        public static readonly Uri ServiceUrl = new Uri("tcp://localhost:9008/MNRP-PuppetMasterService");

        public void CreateWorker(int workerId, string serviceUrl, string entryUrl) {
            var worker = Worker.Run(workerId, new Uri(serviceUrl), workers);
            workers.Add(workerId, worker);

            Trace.WriteLine(string.Format("New worker created: id '{0}', url '{1}'."
                , workerId, worker.ServiceUrl));

            if (!string.IsNullOrWhiteSpace(entryUrl))
                NotifyWorkerCreation(worker);
        }

        public void GetStatus() {
            foreach (IWorker worker in workers.Values){
                worker.GetStatus();
            }
                
        }
        public Dictionary<int, IWorker> GetWorkers() {
            return workers;
        }

        private void NotifyWorkerCreation(Worker worker) {
            Trace.WriteLine("Sends notification to worker at ENTRY_URL informing worker creation.");
            //TODO: Contact worker at ENTRY_URL and announce new worker available.
        }

        /// <summary>
        /// Serves a Marshalled Puppet Master object at a specific IChannel under ChannelServices.
        /// </summary>
        public static void Run() {
            PuppetMasterService service = new PuppetMasterService();
            RemotingHelper.CreateService(service, ServiceUrl);
            Trace.WriteLine("Puppet Master Service listening at '" + ServiceUrl + "'");
        }
    }
}