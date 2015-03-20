using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedTypes;

namespace PuppetMasterLib.Commands
{
    public class SlowWorker : ICommand
    {
        public const string NAME = "sloww";
        public int WorkerId { get; set; }

        public void Execute() {
            //TODO: Implement me.
        }

        public override string ToString() {
            return NAME;
        }
    }
}
