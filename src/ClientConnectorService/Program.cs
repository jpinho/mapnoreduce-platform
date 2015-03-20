using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Threading;

namespace ClientConnectorService
{
    /// <summary>
    /// Bootstrap of Client Connector Services on their respective 
    /// TCP Channels via .NET Remoting.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args) {
            Start();
            Console.ReadKey();
        }

        public static void Start() {
            Console.WriteLine("Bootstrapping Client Services\n");

            Console.WriteLine("MapNoReduce Client Service available at 'tcp://localhost:9005/MNRP-ClientService'");
            new Thread(new ThreadStart(delegate() {
                CreateService<ClientService>(9005, "MNRP-ClientService");
            })).Start();

            Console.WriteLine("MapNoReduce Client Service available at 'tcp://localhost:9006/MNRP-ClientOutputReceiverService'");
            new Thread(new ThreadStart(delegate() {
                CreateService<ClientSplitProviderService>(9006, "MNRP-ClientOutputReceiverService");
            })).Start();

            Console.WriteLine("MapNoReduce Client Service available at 'tcp://localhost:9007/MNRP-ClientSplitProviderService'");
            new Thread(new ThreadStart(delegate() {
                CreateService<ClientOutputReceiverService>(9007, "MNRP-ClientSplitProviderService");
            })).Start();

            Console.WriteLine();
        }

        static void CreateService<T>(int port, string serviceName) where T : MarshalByRefObject, new() {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();

            IDictionary props = new Hashtable();
            props["port"] = port;
            props["typeFilterLevel"] = TypeFilterLevel.Full;
            props["name"] = serviceName;

            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            T service = new T();
            //RemotingServices.Marshal(service, serviceName, service.GetType());
            RemotingConfiguration.RegisterWellKnownServiceType(
                service.GetType(), serviceName,
                WellKnownObjectMode.SingleCall);
        }
    }
}
