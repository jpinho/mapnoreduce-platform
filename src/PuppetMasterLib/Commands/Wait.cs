using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetMasterLib.Commands
{
    public class Wait : ICommand
    {
        public const string NAME = "wait";
        public int Secs { get; set; }

        public void Execute() {
            //TODO: Implement me.
        }

        public override string ToString() {
            return NAME;
        }
    }
}
