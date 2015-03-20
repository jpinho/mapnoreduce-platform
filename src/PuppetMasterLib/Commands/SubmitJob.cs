using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PuppetMasterLib.Commands
{
    public class SubmitJob : ICommand
    {
        public const string NAME = "submit";

        public string EntryURL { get; set; }
        public string FilePath { get; set; }
        public string OutputPath { get; set; }
        public int Splits { get; set; }
        public string MapFunctionPath { get; set; }

        public void Execute() {
            new Thread(new ThreadStart(delegate() {
                ConsoleHelper.CreateConsole();
                UserApplicationSample.Program.Start(
                    EntryURL,
                    FilePath,
                    OutputPath,
                    Splits,
                    MapFunctionPath);
            })).Start();
        }

        public override string ToString() {
            return NAME;
        }
    }
}
