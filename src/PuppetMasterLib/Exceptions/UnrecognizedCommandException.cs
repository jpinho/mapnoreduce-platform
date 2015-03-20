using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetMasterLib.Exceptions
{
    public class UnrecognizedCommandException : Exception
    {
        public UnrecognizedCommandException(string msg)
            : base(msg)
        {

        }
        public UnrecognizedCommandException(string msg, Exception innerException)
            : base(msg, innerException)
        {

        }
    }
}
