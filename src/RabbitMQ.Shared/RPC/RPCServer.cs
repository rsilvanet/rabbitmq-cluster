using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Shared.Messages;
using System;

namespace RabbitMQ.Shared.RPC
{
    public class RPCServer
    {
        private readonly string _id;
        private readonly DateTime _since;

        public RPCServer(IModel channel, string id, DateTime time)
        {
            _id = id;
            _since = time;

            channel.QueueDeclare(
                queue: "rpc_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            channel.BasicQos(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false
            );

            var consumer = new EventingBasicConsumer(channel);

            channel.BasicConsume(
                queue: "rpc_queue",
                autoAck: false,
                consumer: consumer
            );

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                var message = BaseMessage.FromBytes(body);

                if (message.SourceId == _id)
                {
                    channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false
                    );

                    return; // Discards messages from itself
                }

                Console.WriteLine(message.GenerateLog());

                replyProps.CorrelationId = props.CorrelationId;

                channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: props.ReplyTo,
                    basicProperties: replyProps,
                    body: new MasterStatusMessage(_id, _since).GetBytes()
                );

                channel.BasicAck(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false
                );
            };
        }
    }
}
