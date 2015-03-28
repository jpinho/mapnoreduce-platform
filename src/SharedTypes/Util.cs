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
        public const string REGEX_GET_PORT = @":([0-9]{2,5})\/";
        public const string REGEX_GET_PATH = @":[0-9]{2,5}\/(.+)";

        public static int GetHostPort(string url) {
            try {
                MatchCollection portMatches = new Regex(REGEX_GET_PORT).Matches(url);

                bool portMatched = portMatches.Count > 0 && portMatches[0].Groups.Count > 0;

                return int.Parse(portMatches[0].Groups[1].Value);
            } catch (Exception ex) {
                Debug.Write(ex.StackTrace);
            }
            return 0;
        }

        public static string GetServiceName(string url) {
            try {
                MatchCollection serviceNameMatches = new Regex(REGEX_GET_PATH).Matches(url);
                bool serviceMatched = serviceNameMatches.Count > 0 && serviceNameMatches[0].Groups.Count > 0;
                return serviceNameMatches[0].Groups[1].Value.Trim();
            } catch (Exception ex) {
                Debug.Write(ex.StackTrace);
            }
            return string.Empty;
        }
    }
}