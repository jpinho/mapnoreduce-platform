using System;

namespace PuppetMasterLib.Exceptions
{
    public class CommandInvalidParameterException : Exception
    {
        public CommandInvalidParameterException(string msg, Exception innerException)
            : base(msg, innerException) {
        }
    }
}