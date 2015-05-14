using System;
using System.Diagnostics;
using System.Threading;
using PlatformCore.Properties;

namespace PlatformCore
{
    public class Program
    {
        public static void Main(string[] args) {
            Console.WriteLine(Resources.ProgramWelcome);
            Console.WriteLine(Resources.ProgramWelcomeSubtitle);

            var listener = new TextWriterTraceListener(Console.Out);
            Trace.Listeners.Add(listener);

            new Thread(PuppetMasterService.Run).Start();
            Console.ReadKey();
        }
    }
}