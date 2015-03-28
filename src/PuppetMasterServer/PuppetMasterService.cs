using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PlatformCore.Exceptions;
using SharedTypes;

namespace PlatformCore
{
    public class PuppetMasterService : MarshalByRefObject, IPuppetMasterService
    {
        public const int SERVER_PORT = 9008;
        public const string SERVER_NAME = "MNRP-PuppetMasterService";

        private Dictionary<int, IWorker> workers = new Dictionary<int, IWorker>();
        private static PuppetMasterService serviceInstance;

        public static bool IsInitialized { get; private set; }

        public void CreateWorker(int workerId, string serviceURL, string entryURL) {
            int servicePort = Util.GetHostPort(serviceURL);
            string serviceName = Util.GetServiceName(serviceURL);

            if (servicePort == 0 && string.IsNullOrWhiteSpace(serviceName))
                throw new InvalidWorkerServiceUrlException(workerId, serviceURL);

            Worker worker = new Worker(workerId, servicePort, serviceName);
            worker.Run();
            workers.Add(workerId, worker);

            Debug.WriteLine(string.Format(
                "New worker created at Puppet Master: id '{0}', port '{1}', service name '{2}', url '{3}'."
                , workerId
                , servicePort
                , serviceName
                , worker.GetWorkerURL()));

            if (!string.IsNullOrWhiteSpace(entryURL))
                NotityWorkerCreation(worker);
        }

        public Dictionary<int, IWorker> GetWorkers() {
            return workers;
        }

        private void NotityWorkerCreation(Worker worker) {
            Debug.WriteLine("Sends notification to worker at ENTRY_URL informing worker creation.");
            //TODO: Contact ENTRY_URL worker.
        }

        /// <summary>
        /// Servers a Marshled Puppet Master object at a specific IChannel under ChannelServices.
        /// Such service is runned as a singleton, multiple Puppet Master object marshalls are not supported.
        /// </summary>
        public static void Run() {
            if (IsInitialized)
                return;

            serviceInstance = RemotingHelper.CreateService<PuppetMasterService>(
                PuppetMasterService.SERVER_PORT,
                PuppetMasterService.SERVER_NAME);

            IsInitialized = true;
        }

        public static string GetMasterURL() {
            if (!IsInitialized)
                return null;

            return string.Format("tcp://localhost:{0}/{1}",
                PuppetMasterService.SERVER_PORT,
                PuppetMasterService.SERVER_NAME);
        }
    }
}