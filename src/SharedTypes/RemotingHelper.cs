using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;

namespace SharedTypes
{
	public class RemotingHelper
	{
		public static void CreateService(Object remoteObject, Uri serviceUrl) {
			CreateService(remoteObject, serviceUrl, false);
		}

		public static void CreateService(Object remoteObject, Uri serviceUrl, bool registerChannel) {
			if (registerChannel) {
				try {
					RegisterChannel(serviceUrl);
				} catch (Exception ex) {
					Trace.TraceError("RegisterChannel failed - " + ex.Message + " -->> " + ex.StackTrace);
				}
			}

			//RemotingServices.Marshal(
			//	(MarshalByRefObject)remoteObject
			//	, serviceUrl.AbsolutePath.TrimStart('/')
			//	, remoteObject.GetType());

			RemotingConfiguration.RegisterWellKnownServiceType(
				remoteObject.GetType(),
				serviceUrl.AbsolutePath.TrimStart('/'),
				WellKnownObjectMode.Singleton);
		}

		public static void RegisterChannel(Uri serviceUrl) {
			var provider = new BinaryServerFormatterSinkProvider();

			IDictionary props = new Hashtable();
			props["port"] = serviceUrl.Port;
			props["typeFilterLevel"] = TypeFilterLevel.Full;
			props["name"] = serviceUrl.AbsolutePath.TrimStart('/');

			var channel = new TcpChannel(props, null, provider);
			ChannelServices.RegisterChannel(channel, true);
		}

		public static T GetRemoteObject<T>(string serviceUrl) {
			return (T)Activator.GetObject(typeof(T), serviceUrl);
		}

		public static T GetRemoteObject<T>(Uri serviceUrl) {
			return (T)Activator.GetObject(typeof(T), serviceUrl.AbsoluteUri);
		}
	}
}