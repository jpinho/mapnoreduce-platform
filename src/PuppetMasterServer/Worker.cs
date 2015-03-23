using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformServer
{
    class Worker
    {
        public int WorkerId { get; set; }

        public Worker() {
            Console.WriteLine("Faz de conta que criei um worker.");
        }
    }
}
