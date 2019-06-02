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
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
            };

            var channel = factory.CreateConnection().CreateModel();
            var client = new RPCClient(channel);
            var cTokenSource = new CancellationTokenSource();
            cTokenSource.CancelAfter(5000);

            try
            {
                Console.WriteLine("Searching for the master...");
                var message = await client.CallAsync("Hello from the client.", cTokenSource.Token);
                Console.WriteLine(message);
            }
            catch (OperationCanceledException)
            {
                new RPCServer(channel);
                Console.WriteLine("Master not found. You're the master now.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.Read();
        }
    }
}
