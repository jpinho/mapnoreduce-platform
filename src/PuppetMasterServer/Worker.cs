using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMasterServer
{
    internal class Worker
    {
        public int WorkerId { get; set; }

        public Worker() {
            Debug.WriteLine("Faz de conta que criei um worker.");
        }
    }
}