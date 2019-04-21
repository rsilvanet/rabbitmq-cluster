using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Shared.Commands;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Shared
{
    public static class Service
    {
        private static Node master;
        private static Node current;
        private static IModel channel;

        private static void Publish<T>(string queue, T content)
        {
            var json = JsonConvert.SerializeObject(content);
            var message = Encoding.UTF8.GetBytes(json);

            channel.QueueDeclare(queue, false, false, false);
            channel.BasicPublish(string.Empty, queue, null, message);
        }

        private static void SignToChannels()
        {
            channel.QueueDeclare(Queues.ASK_MASTER, false, false, false);
            channel.QueueDeclare(Queues.ASK_MASTER_RESPONSE, false, false, false);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                if (ea.RoutingKey == Queues.ASK_MASTER)
                {
                    if (current.Master)
                    {
                        Publish(Queues.ASK_MASTER_RESPONSE, current);
                    }
                }
                else if (ea.RoutingKey == Queues.ASK_MASTER_RESPONSE)
                {
                    var json = Encoding.UTF8.GetString(ea.Body);

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        master = JsonConvert.DeserializeObject<Node>(json);
                    }
                }
            };

            channel.BasicConsume(Queues.ASK_MASTER, true, consumer);
            channel.BasicConsume(Queues.ASK_MASTER_RESPONSE, true, consumer);
        }

        public static string ShowMyId()
        {
            return current?.Id;
        }

        public static void StartNewNode()
        {
            current = new Node()
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

            channel = factory.CreateConnection().CreateModel();

            SignToChannels();
            AskForMaster(current.Id);
        }

        public static void AskForMaster(string requesterId)
        {
            Publish(Queues.ASK_MASTER, new AskForMasterCmd(requesterId));

            Task.Delay(5000).Wait();

            if (master == null)
            {
                current.Master = true;
            }
        }
    }
}
