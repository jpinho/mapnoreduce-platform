using System;

namespace SharedTypes
{
	public static class Globals
	{
		public static Uri LocalPuppetMasterUri = new Uri("TCP://" + Util.GetHostIpAddress() + ":20001/PM");
	}
}