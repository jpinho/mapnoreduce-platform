using System;
using System.Runtime.Remoting;
using System.Runtime.Serialization;

namespace PlatformCore.Exception
{
    [Serializable]
    public class InvalidWorkerIdException : RemotingException
    {
        private readonly string internalMessage;

        public InvalidWorkerIdException() {
        }

        public InvalidWorkerIdException(int workerId, string innerException)
            : base(string.Format("The worker id '{0}' is invalid - '{1}'.",
                workerId, innerException)) {
            internalMessage = base.Message;
        }

        public InvalidWorkerIdException(SerializationInfo info, StreamingContext context) {
            internalMessage = (string)info.GetValue("internalMessage", typeof(string));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("internalMessage", internalMessage);
        }

        // Returns the exception information.
        public override string Message {
            get { return internalMessage; }
        }
    }
}