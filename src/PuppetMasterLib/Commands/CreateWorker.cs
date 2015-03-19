using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMasterLib.Commands
{
    public class CreateWorker: ICommand
    {
        public const string NAME = "worker";

        public int WorkerId { get; set; }
        public string PuppetMasterURL { get; set; }
        public string ServiceURL { get; set; }
        public string EntryURL { get; set; }

        public T execute<T>()
        {
            throw new NotImplementedException();
        }
    }
}
