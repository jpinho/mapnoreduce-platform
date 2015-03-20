using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMasterLib.Helpers
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    public static class ConsoleHelper
    {
        public static void CreateConsole() {
            AllocConsole();

            TextWriter writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(writer);
        }

        [DllImport("kernel32")]
        static extern bool AllocConsole();
    }
}
