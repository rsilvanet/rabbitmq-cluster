using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Shared.Commands;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Shared
{
    public static class Service
    {
        private static Node master;
        private static Node current;
        private static IModel channel;
        private static BlockingCollection<string> blocking;

        private static void Publish<T>(string queue, T content)
        {
            var json = JsonConvert.SerializeObject(content);
            var message = Encoding.UTF8.GetBytes(json);
            var props = channel.CreateBasicProperties();

            props.ReplyTo = $"{queue}_reply";

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: queue,
                basicProperties: null,
                body: message
                );
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
            blocking = new BlockingCollection<string>();

            //SignToChannels();
            AskForMaster();
        }

        public static void AskForMaster()
        {
            var replyQueueName = channel.QueueDeclare().QueueName;
            var consumer = new EventingBasicConsumer(channel);
            var props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var response = Encoding.UTF8.GetString(body);

                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    blocking.Add(response);
                }
            };

            channel.BasicPublish(string.Empty, Queues.ASK_MASTER, props, null);
            channel.BasicConsume(consumer, replyQueueName, true);

            blocking.Take();
        }

        public static void AssignForAskMaster()
        {
            channel.QueueDeclare(Queues.ASK_MASTER);
            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);

            channel.BasicConsume(Queues.ASK_MASTER, false, consumer);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var props = ea.BasicProperties;
                var response = string.Empty;
                var replyProps = channel.CreateBasicProperties();

                replyProps.CorrelationId = props.CorrelationId;

                if (current.Master)
                {
                    response = JsonConvert.SerializeObject(current);
                }

                channel.BasicPublish(string.Empty, props.ReplyTo, replyProps, Encoding.UTF8.GetBytes(response));
                channel.BasicAck(ea.DeliveryTag, false);
            };
        }
    }
}
