using RabbitMQ.Client;
using RabbitMQ.Shared.RPC;
using System;
using System.Net.Http;
using System.Threading;
namespace RabbitMQ.Node
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var cmd = Console.ReadLine();

            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
            };

            var channel = factory.CreateConnection().CreateModel();

            if (cmd == "S")
            {
                new RPCServer(channel);
                Console.WriteLine("This one is a server now.");
            }
            else if (cmd == "C")
            {
                var client = new RPCClient(channel);
                var cTokenSource = new CancellationTokenSource();
                Console.WriteLine("This one is a client now.");
                var message = client.Call("Hello from the client!", cTokenSource.Token).Result;
                Console.WriteLine(message);
            }

            Console.Read();
        }
    }
}
