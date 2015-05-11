using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace SharedTypes
{
	public static class Util
	{
		public const string LOCALHOST = "127.0.0.1";

		public static int GetHostPort(string url) {
			return new Uri(url).Port;
		}

		public static string GetServiceName(string url) {
			return new Uri(url).Host;
		}

		public static string GetHostIpAddress() {
			var hostName = Dns.GetHostName();
			var ipaddress = Dns.GetHostAddresses(hostName);
			foreach (var ip4 in ipaddress.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork))
				return ip4.ToString();
			return LOCALHOST;
		}
	}
}