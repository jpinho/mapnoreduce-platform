using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SharedTypes
{
    public static class Util
    {
        public static int GetHostPort(string url) {
            return new Uri(url).Port;
        }

        public static string GetServiceName(string url) {
            return new Uri(url).Host;
        }
    }
}