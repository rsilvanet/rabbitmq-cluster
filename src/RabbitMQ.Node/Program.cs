using RabbitMQ.Client;
using RabbitMQ.Shared.RPC;
using System;
using System.Threading
    ;
using System.Threading.Tasks;

namespace RabbitMQ.Node
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("What will it be (C/S)?");

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
                cTokenSource.CancelAfter(5000);

                try
                {
                    Console.WriteLine("This one is a client now.");
                    var message = await client.CallAsync("Hello from the client!", cTokenSource.Token);
                    Console.WriteLine(message);
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine($"No server found: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Console.Read();
        }
    }
}
