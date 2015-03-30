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
        public int WorkerId { get; private set; }
        public int HostPort { get; private set; }
        public string ServiceName { get; private set; }
        public bool IsInitialized { get; private set; }
        public JobTracker tracker = null;
        public Worker() {
        }

        public Worker(int workerId, int hostPort, string serviceName) {
            this.WorkerId = workerId;
            this.HostPort = hostPort;
            this.ServiceName = serviceName;
        }


        #region IWorker Members


        public void ReceiveMapJob(string filePath, int nSplits, byte[] mapAssemblyCode, string mapClassName) {
            new Thread(new ThreadStart(delegate
            {
                tracker = new JobTracker(this, JobTracker.JobTrackerStatus.ACTIVE);
                tracker.start();
            })).Start();
        }

        public bool ExecuteMapJob(IJobTask task) {
            
            new Thread(new ThreadStart(delegate
            {
                tracker = new JobTracker(this, JobTracker.JobTrackerStatus.PASSIVE);
                tracker.start();
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

        #endregion IWorker Members

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

        internal new List<IWorker> getActiveWorkers()
        {
            throw new NotImplementedException();
        }
    }
}