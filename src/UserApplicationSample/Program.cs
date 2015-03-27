using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserApplicationSample
{
    public class Program
    {
        private static string HELP = Resources.HelpMessage;

        public static void Main(string[] args) {
            if (args.Length == 1)
                Console.Out.Write(HELP);

            string entryURL = args[1];
            string file = args[2];
            string ouput = args[3];
            int nSplits = Int32.Parse(args[4]);
            string map = args[5];
            string dll = args[6];

            Start(entryURL, file, ouput, nSplits, map, dll);
        }

        public static void Start(string EntryURL, string FilePath, string OutputPath, int Splits, string MapFunctionPath, string Dll) {
            Console.WriteLine("User App started as:");
            Console.WriteLine("> ./UserApp -EntryURL={0} -FilePath={1} -OutputPath={2} -Splits={3} -MapFunctionPath={4} -Dll={5}",
                EntryURL, FilePath, OutputPath, Splits.ToString(), MapFunctionPath, Dll);
            System.Threading.Thread.Sleep(5000);
        }
    }
}