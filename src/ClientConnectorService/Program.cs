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

            int portNumber = 9005;
            ClientService service = new ClientService();
            ClientHelper.CreateService<ClientService>(service, service.ToString(), portNumber);
            Console.WriteLine("MapNoReduce Client Service available at 'tcp://localhost:" + portNumber + "/" + service.ToString() + "'.\n");
        }
    }
}
