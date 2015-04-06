using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlatformCore
{
    public class InvalidWorkerIdException : Exception
    {
        public InvalidWorkerIdException(int workerId, Exception innerException)
            : base(string.Format("The worker id '{0}' is invalid.",
                workerId), innerException) { }
    }
}
