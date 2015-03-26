using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlatformServer
{
    public class Program
    {
        public static void Main(string[] args) {
            Start();
            Console.ReadKey();
        }

        public static void Start() {
            new Thread(new ThreadStart(delegate() {
                CreateService<PuppetMasterService>(9008, "MNRP-PuppetMasterService");
            })).Start();
        }

        private static void CreateService<T>(int port, string serviceName) where T : MarshalByRefObject, new() {
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
        }
    }
}
