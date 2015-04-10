using System;

namespace SharedTypes
{
    public class MnrTextTraceListener : System.Diagnostics.TextWriterTraceListener
    {
        private const string MASK = "MAPNOREDUCE :: {0} :: ";
        private const string TFORMAT = "dd-MM-yyTHH:mm:ss";

        public MnrTextTraceListener(string file)
            : base(file) {
        }

        public override void WriteLine(string message) {
            this.Write(string.Format(MASK, DateTime.Now.ToString(TFORMAT)));
            base.WriteLine(message);
        }
    }
}