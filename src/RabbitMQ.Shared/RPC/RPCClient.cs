using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Shared.Messages;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Shared.RPC
{
    public class RPCClient
    {
        private readonly IModel _channel;
        private readonly string _replyQueueName;
        private readonly EventingBasicConsumer _consumer;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<BaseMessage>> _callbackMapper;

        public RPCClient(IModel channel)
        {
            _channel = channel;
            _replyQueueName = channel.QueueDeclare().QueueName;
            _consumer = new EventingBasicConsumer(channel);
            _callbackMapper = new ConcurrentDictionary<string, TaskCompletionSource<BaseMessage>>();

            _consumer.Received += (model, ea) =>
            {
                if (!_callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
                {
                    return;
                }

                tcs.TrySetResult(BaseMessage.FromBytes(ea.Body));
            };
        }

        public async Task<BaseMessage> CallAsync(BaseMessage message, CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();

            var coId = Guid.NewGuid().ToString();
            var props = _channel.CreateBasicProperties();
            props.CorrelationId = coId;
            props.ReplyTo = _replyQueueName;

            var tcs = new TaskCompletionSource<BaseMessage>();

            _callbackMapper.TryAdd(coId, tcs);

            _channel.BasicPublish(
                exchange: "",
                routingKey: "rpc_queue",
                basicProperties: props,
                body: message.GetBytes()
            );

            _channel.BasicConsume(
                consumer: _consumer,
                queue: _replyQueueName,
                autoAck: true
            );

            cToken.Register(() => _callbackMapper.TryRemove(coId, out var _));

            tcs.Task.Wait(cToken);

            var response = await tcs.Task;

            Console.WriteLine(response.GenerateLog());

            return response;
        }
    }
}
