using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlatformServer
{
    public class Program
    {
        public static void Main(string[] args) {
            new Thread(new ThreadStart(delegate() {
                PuppetMasterService.Run();
            })).Start();

            Console.WriteLine("PuppetMasterService being started... give it a couple seconds!");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}