using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientServices;
using SharedTypes;

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
            string mapClassName = args[5];
            string assemblyFilePath = args[6];

            ExecuteMapJob(entryURL, file, ouput, nSplits, mapClassName, assemblyFilePath);
        }

        public static void ExecuteMapJob(string entryURL, string filePath, string outputPath, int splits, string mapClassName, string assemblyFilePath) {
            Console.WriteLine("User Application, started with the following parameters:\n"
                + "-EntryURL={0} -FilePath={1} -OutputPath={2} -Splits={3} -MapClassName={4} -AssemblyFilePath={5}",
                entryURL, filePath, outputPath, splits.ToString(), mapClassName, assemblyFilePath);

            ClientService client = new ClientService();
            client.Init(entryURL);
            client.Submit(filePath, splits, outputPath, mapClassName, assemblyFilePath);
        }
    }
}