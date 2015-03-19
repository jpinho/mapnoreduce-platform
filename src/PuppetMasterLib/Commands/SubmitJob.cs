using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            //TODO: Implement me.
        }

        public override string ToString() {
            return NAME;
        }
    }
}
