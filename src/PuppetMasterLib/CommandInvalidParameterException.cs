using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuppetMasterLib
{
    public class CommandInvalidParameterException : Exception
    {
        public CommandInvalidParameterException(string msg, Exception innerException)
            : base(msg, innerException)
        {
        }
    }
}
