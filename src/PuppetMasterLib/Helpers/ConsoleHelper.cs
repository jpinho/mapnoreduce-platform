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
            SetConsoleCtrlHandler(null, true);
        }

        [DllImport("kernel32")]
        public static extern bool AllocConsole();

        [DllImport("kernel32")]
        public static extern bool FreeConsole();

        [DllImport("kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine HandlerRoutine, bool Add);

        public delegate bool HandlerRoutine(uint dwControlType);
    }
}
