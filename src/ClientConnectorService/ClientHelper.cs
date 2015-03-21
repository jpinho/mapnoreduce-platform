using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;

namespace ClientConnectorService
{
    class ClientHelper
    {
        public static void CreateService<T>(T serviceInstance, string serviceName, int port) where T : MarshalByRefObject {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();

            IDictionary props = new Hashtable();
            props["port"] = port;
            props["typeFilterLevel"] = TypeFilterLevel.Full;
            props["name"] = serviceName;

            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(
                serviceInstance.GetType(), serviceName,
                WellKnownObjectMode.Singleton);
        }
    }
}
