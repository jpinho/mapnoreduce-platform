using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetMasterLib.Commands
{
    public class Status : ICommand
    {
        public const string NAME = "status";

        public void Execute() {
            //TODO: Implement me.
        }

        public override string ToString() {
            return NAME;
        }
    }
}
