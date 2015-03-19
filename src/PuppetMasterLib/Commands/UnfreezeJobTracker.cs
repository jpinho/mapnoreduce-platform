using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetMasterLib.Commands
{
    public class UnfreezeJobTracker : ICommand
    {
        public const string NAME = "unfreezec";
        public int WorkerId { get; set; }

        public void Execute() {
            //TODO: Implement me.
        }

        public override string ToString() {
            return NAME;
        }
    }
}
