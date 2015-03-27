using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using SharedTypes;

namespace PlatformCore
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

        public bool ExecuteMapJob(string dataToProcess, byte[] code, string className) {
            Assembly assembly = Assembly.Load(code);

            foreach (Type type in assembly.GetTypes()) {
                if (type.IsClass == true) {
                    if (type.FullName.EndsWith("." + className)) {
                        object mapperClassObj = Activator.CreateInstance(type);

                        object[] args = new object[] { dataToProcess };
                        object resultObject = type.InvokeMember("Map",
                          BindingFlags.Default | BindingFlags.InvokeMethod,
                               null,
                               mapperClassObj,
                               args);
                        IList<KeyValuePair<string, string>> result = (IList<KeyValuePair<string, string>>)resultObject;

                        Console.WriteLine("Map call result was: ");
                        foreach (KeyValuePair<string, string> p in result) {
                            Console.WriteLine("key: " + p.Key + ", value: " + p.Value);
                        }
                        return true;
                    }
                }
            }
            return false;
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
            RemotingHelper.CreateService<Worker>(HostPort, ServiceName);
            IsInitialized = true;
        }
    }
}