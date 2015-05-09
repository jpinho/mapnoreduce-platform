using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
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
        public static int GetHostPort(string url)
        {
            return new Uri(url).Port;
        }

        public static string GetServiceName(string url)
        {
            return new Uri(url).Host;
        }

        public static string GetHostIpAddress()
        {
            string HostName = Dns.GetHostName();
            IPAddress[] ipaddress = Dns.GetHostAddresses(HostName);
            foreach (IPAddress ip4 in ipaddress.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
                return ip4.ToString();
            return "127.0.0.1";
        }
    }
}