using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using SharedTypes;

namespace PlatformServer
{
    public class PuppetMasterService : MarshalByRefObject, IPuppetMasterService
    {
        public void createWorker()
        {
            Worker w = new Worker();
        }
    }
}
