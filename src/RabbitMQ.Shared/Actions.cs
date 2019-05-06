using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Shared
{
    public class Actions
    {
        private void Publish<T>(string queue, T content)
        {
            var json = JsonConvert.SerializeObject(content);
            var message = Encoding.UTF8.GetBytes(json);

            State.Channel.BasicPublish(
                exchange: string.Empty,
                routingKey: queue,
                basicProperties: null,
                body: message
            );
        }

        private void SignToChannels()
        {
            State.Channel.QueueDeclare(
                queue: Queues.ASK_FOR_MASTER,
                durable: false,
                exclusive: false,
                autoDelete: false
            );

            var consumer = new EventingBasicConsumer(State.Channel);

            consumer.Received += (model, ea) =>
            {
                if (ea.RoutingKey == "queue_name_here")
                {
                    // Publish(response)
                }
            };

            State.Channel.BasicConsume(Queues.ASK_FOR_MASTER, true, consumer);
        }

        public static string ShowMyId()
        {
            return State.Current?.Id;
        }

        public static bool IsTheMaster()
        {
            return State.Current?.Id == State.Master?.Id;
        }

        public static async Task StartNewNode()
        {
            State.Current = new Node()
            {
                Id = Guid.NewGuid().ToString(),
                StartTime = DateTime.Now,
                Status = Node.EStatus.Created,
                Master = false
            };

            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
            };

            State.Channel = factory.CreateConnection().CreateModel();
            State.RPCServer = new RPC.RPCServer(State.Current, State.Channel);
            State.RPCClient = new RPC.RPCClient(State.Channel);

            await AskForMaster();
        }

        public static async Task AskForMaster()
        {
            var cts = new CancellationTokenSource(2_000);

            try
            {
                var response = await State.RPCClient.CallAsync($"{State.Current.Id}: Who is the master?", cts.Token);

                Console.WriteLine(response);
            }
            catch (OperationCanceledException ex)
            {
                State.Current.Master = true;
                State.Master = State.Current;
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
