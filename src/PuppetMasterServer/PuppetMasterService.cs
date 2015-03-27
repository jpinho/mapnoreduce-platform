using SharedTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace PlatformServer
{
    public class PuppetMasterService : MarshalByRefObject, IPuppetMasterService
    {
        private List<Worker> workers = new List<Worker>();

        public void createWorker() {
            Debug.WriteLine("New worker created at Puppet Master.");
            workers.Add(new Worker());
        }
    }
}