using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMQ.Shared.RPC
{
    public class RPCServer
    {
        public RPCServer(IModel channel)
        {
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
                var message = Encoding.UTF8.GetString(body);

                replyProps.CorrelationId = props.CorrelationId;

                channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: props.ReplyTo,
                    basicProperties: replyProps,
                    body: Encoding.UTF8.GetBytes($"Hi, I'm the server!")
                );

                channel.BasicAck(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false
                );
            };
        }
    }
}
