using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PuppetMasterLib.Commands;
using SharedTypes;

namespace PlatformCore
{
    public class Worker : MarshalByRefObject, IWorker
    {
        public Uri ServiceUrl { get; set; }
        public int WorkerId { get; set; }

        public Worker() {
        }

        public Worker(int workerId, Uri serviceUrl) {
            WorkerId = workerId;
            ServiceUrl = serviceUrl;
        }

        public bool ExecuteMapJob(IJobTask task) {
            // TODO: ask client split provider for split data (task.SplitProviderURL, task.SplitNumber)
            string data = "";

            Assembly assembly = Assembly.Load(task.MapFunctionAssembly);

            foreach (Type type in assembly.GetTypes()) {
                if (type.IsClass == true) {
                    if (type.FullName.EndsWith("." + task.MapClassName)) {
                        object mapperClassObj = Activator.CreateInstance(type);

                        object[] args = new object[] { data };
                        object resultObject = type.InvokeMember("Map",
                          BindingFlags.Default | BindingFlags.InvokeMethod,
                               null,
                               mapperClassObj,
                               args);
                        IList<KeyValuePair<string, string>> result = (IList<KeyValuePair<string, string>>)resultObject;

                        Debug.WriteLine("Map call result was: ");
                        foreach (KeyValuePair<string, string> p in result) {
                            Debug.WriteLine("key: " + p.Key + ", value: " + p.Value);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public void ReceiveMapJob(string filePath, int nSplits, byte[] mapAssemblyCode, string mapClassName) {
            //TODO
            Thread.Sleep(10 * 1000);
        }

        internal static Worker Run(int workerId, Uri serviceUrl) {
            Worker worker = new Worker(workerId, serviceUrl);
            RemotingHelper.CreateService(worker, serviceUrl);
            Debug.Write(string.Format("Creating new worker. Worker service will be listenning at '{0}'.", serviceUrl));
            return worker;
        }
    }
}