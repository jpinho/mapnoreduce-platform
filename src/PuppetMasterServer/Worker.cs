using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharedTypes;

namespace PlatformCore
{
    public class Worker : MarshalByRefObject, IWorker
    {
        private JobTracker tracker = null;

        public int WorkerId { get; set; }
        public Uri ServiceUrl { get; set; }

        public Worker(int workerId, Uri serviceUrl) {
            WorkerId = workerId;
            ServiceUrl = serviceUrl;
        }

        public void ReceiveMapJob(string filePath, int nSplits, byte[] mapAssemblyCode, string mapClassName) {

            new Thread(new ThreadStart(delegate {
                tracker = new JobTracker(this);
                tracker.Start(JobTracker.JobTrackerStatus.ACTIVE);
            })).Start();
        }

        public bool ExecuteMapJob(IJobTask task) {
            new Thread(new ThreadStart(delegate {
                tracker = new JobTracker(this);
                tracker.Start(JobTracker.JobTrackerStatus.PASSIVE);
            })).Start();

            IClientSplitProviderService splitProvider = (IClientSplitProviderService)Activator.GetObject(
              typeof(IClientSplitProviderService),
              task.SplitProviderURL);

            string data = splitProvider.GetFileSplit(task.FileName, int.Parse(task.SplitNumber));

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

        internal static Worker Run(int workerId, Uri serviceUrl) {
            var wrk = new Worker(workerId, serviceUrl);
            RemotingHelper.CreateService(wrk, serviceUrl);
            return wrk;
        }

        internal Dictionary<int, IWorker> GetActiveWorkers()
        {
            throw new NotImplementedException();
        }
    }
}