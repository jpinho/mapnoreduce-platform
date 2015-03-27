using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharedTypes
{
    public class RemotingHelper
    {
        public static T CreateService<T>(int port, string serviceName) where T : MarshalByRefObject, new() {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();

            IDictionary props = new Hashtable();
            props["port"] = port;
            props["typeFilterLevel"] = TypeFilterLevel.Full;
            props["name"] = serviceName;

            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            T service = new T();
            RemotingConfiguration.RegisterWellKnownServiceType(
                service.GetType(), serviceName,
                WellKnownObjectMode.Singleton);

            return service;
        }

        public static T GetRemoteObject<T>(string serviceName, string serviceURL) {
            return (T)Activator.GetObject(typeof(T), serviceURL);
        }
    }
}