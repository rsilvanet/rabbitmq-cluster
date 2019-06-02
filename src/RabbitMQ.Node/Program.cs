using RabbitMQ.Client;
using RabbitMQ.Shared.Messages;
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
            var discover = new DiscoverMessage();

            Console.WriteLine("Welcome!");
            Console.WriteLine("Your ID: " + discover.SourceId);
            Console.WriteLine("Your Time: " + discover.Time);

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
                Console.WriteLine("Looking for the master...");
                await client.CallAsync(discover, cTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                new RPCServer(channel, discover.SourceId, DateTime.Now);
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
