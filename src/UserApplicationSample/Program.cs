using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserApplicationSample
{
    public class Program
    {
        static string HELP = @"
##############################
#  User Application Sample
#  Version: 1.0
##############################

 Parameters:

    <ENTRY-URL>     The URL of a worker node to which the client can connect to submit jobs.

    <FILE>          Is the path to the input file. 
                    The file will be subdivided into <S> splits across the machines in W.

    <OUTPUT>        Is the path to an output directory on the local filesystem of the application, 
                    which will store one output file for each split of the input file name 'S1.out', 
                    'S2.out', ..., 'S.out'.

    <S>             The number of splits of the input file, which corresponds to the total number 
                    of worker tasks to be executed.

    <MAP>           The name of the class implementing the IMap interface.

 Usage Example:

    UserApplicationSample.exe <ENTRY-URL> <FILE> <OUTPUT> <S> <MAP>

";

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
