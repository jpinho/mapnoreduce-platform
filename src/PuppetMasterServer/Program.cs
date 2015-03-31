using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlatformCore
{
    public class Program
    {
        public static void Main(string[] args) {
            Console.WriteLine("Puppet Master Service started!\nPress any key to exit.\n");
            Console.WriteLine("## Application Trace ##\n");

            TextWriterTraceListener listener = new TextWriterTraceListener(System.Console.Out);
            Trace.Listeners.Add(listener);

            new Thread(new ThreadStart(PuppetMasterService.Run)).Start();
            Console.ReadKey();
        }
    }
}