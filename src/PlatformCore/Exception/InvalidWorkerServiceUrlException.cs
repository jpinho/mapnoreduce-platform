namespace PlatformCore.Exception
{
    public class InvalidWorkerServiceUrlException : System.Exception
    {
        public InvalidWorkerServiceUrlException(int workerId, string serviceUrl)
            : base(string.Format("The service URL '{0}' for worker '{1}' is invalid.",
                serviceUrl, workerId)) { }
    }
}