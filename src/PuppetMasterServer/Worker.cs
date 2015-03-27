using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using SharedTypes;

namespace PlatformServer
{
    public class Worker : MarshalByRefObject, IWorker
    {
        public int WorkerId { get; private set; }
        public int HostPort { get; private set; }
        public string ServiceName { get; private set; }
        public bool IsInitialized { get; private set; }

        public Worker() {
        }

        public Worker(int workerId, int hostPort, string serviceName) {
            this.WorkerId = workerId;
            this.HostPort = hostPort;
            this.ServiceName = serviceName;
        }

        /// <summary>
        /// Returns the worker service URL.
        /// </summary>
        public string GetWorkerURL() {
            if (!IsInitialized)
                return null;
            return string.Format("tcp://localhost:{0}/{1}",
                HostPort, ServiceName);
        }

        internal void Run() {
            if (IsInitialized)
                return;
            Helper.CreateService<Worker>(HostPort, ServiceName);
            IsInitialized = true;
        }
    }
}