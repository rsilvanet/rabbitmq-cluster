using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Shared.RPC
{
    public class RPCClient
    {
        private readonly IModel _channel;
        private readonly string _replyQueueName;
        private readonly EventingBasicConsumer _consumer;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper;

        public RPCClient(IModel channel)
        {
            _channel = channel;
            _replyQueueName = channel.QueueDeclare().QueueName;
            _consumer = new EventingBasicConsumer(channel);
            _callbackMapper = new ConcurrentDictionary<string, TaskCompletionSource<string>>();

            _consumer.Received += (model, ea) =>
            {
                if (!_callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
                {
                    return;
                }

                tcs.TrySetResult(Encoding.UTF8.GetString(ea.Body));
            };
        }

        public async Task<string> CallAsync(string message, CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();

            var coId = Guid.NewGuid().ToString();
            var props = _channel.CreateBasicProperties();
            props.CorrelationId = coId;
            props.ReplyTo = _replyQueueName;

            var tcs = new TaskCompletionSource<string>();

            _callbackMapper.TryAdd(coId, tcs);

            _channel.BasicPublish(
                exchange: "",
                routingKey: "rpc_queue",
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(message)
            );

            _channel.BasicConsume(
                consumer: _consumer,
                queue: _replyQueueName,
                autoAck: true
            );

            cToken.Register(() => _callbackMapper.TryRemove(coId, out var _));

            tcs.Task.Wait(cToken);

            return await tcs.Task;
        }
    }
}
