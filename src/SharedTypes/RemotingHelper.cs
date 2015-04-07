using System;
using System.Collections;
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
            if (!registerChannel) {
                BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();

                IDictionary props = new Hashtable();
                props["port"] = serviceUrl.Port;
                props["typeFilterLevel"] = TypeFilterLevel.Full;
                props["name"] = serviceUrl.AbsolutePath.TrimStart('/');
                

                TcpChannel channel = new TcpChannel(props, null, provider);
                ChannelServices.RegisterChannel(channel, true);
            }

            RemotingConfiguration.RegisterWellKnownServiceType(
                remoteObject.GetType(), serviceUrl.AbsolutePath.TrimStart('/'),
                WellKnownObjectMode.Singleton);
        }

        public static T GetRemoteObject<T>(string serviceUrl) {
            return (T)Activator.GetObject(typeof(T), serviceUrl);
        }
    }
}