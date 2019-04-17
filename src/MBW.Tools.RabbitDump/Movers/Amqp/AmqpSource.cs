using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using MBW.Tools.RabbitDump.Options;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MBW.Tools.RabbitDump.Movers.Amqp
{
    class AmqpSource : ISource, IDisposable
    {
        private readonly ArgumentsModel _model;
        private readonly ILogger<AmqpSource> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private int _remaining;
        private bool _finished;

        public AmqpSource(ArgumentsModel model, ILogger<AmqpSource> logger)
        {
            _model = model;
            _logger = logger;
            _remaining = model.MessageLimit ?? -1;

            UriConnectionFactory connectionFactory = new UriConnectionFactory(new Uri(model.Input));

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void SendData(ITargetBlock<MessageItem> target, CancellationToken cancellationToken)
        {
            ushort prefetch = _model.BatchSize <= ushort.MaxValue ? (ushort)_model.BatchSize : ushort.MaxValue;

            using (CancellationTokenSource internalSource = new CancellationTokenSource())
            using (CancellationTokenSource combinedSource = CancellationTokenSource.CreateLinkedTokenSource(internalSource.Token, cancellationToken))
            {
                _channel.BasicQos(0, prefetch, true);

                ManualResetEventSlim waitingForMessage = new ManualResetEventSlim(false);
                AutoResetEvent messageSent = new AutoResetEvent(false);

                cancellationToken.Register(() => messageSent.Set());

                EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (sender, item) =>
                {
                    if (_finished)
                        return;

                    waitingForMessage.Set();
                    MessageItem data = Convert(item);

                    target.SendAsync(data, combinedSource.Token).GetAwaiter().GetResult();

                    if (_remaining != -1)
                    {
                        _remaining--;
                        if (_remaining == 0)
                        {
                            _finished = true;
                            internalSource.Cancel();
                        }
                    }

                    messageSent.Set();
                    waitingForMessage.Reset();
                };

                foreach (string queue in _model.Arguments)
                    _channel.BasicConsume(queue, false, consumer);

                // Wait for consumer to finish
                if (_model.UseContinuous)
                {
                    // Continuous: Wait for cancellationToken
                    _logger.LogInformation("In Continuous mode, press ctrl+c to cancel the transfer");

                    combinedSource.Token.WaitHandle.WaitOne();

                    _channel.BasicCancel(consumer.ConsumerTag);
                }
                else if (_model.UseOneShot)
                {
                    // One-shot: Wait for cancellationToken, or no message from source for X seconds
                    TimeSpan idleTime = TimeSpan.FromSeconds(5);

                    _logger.LogInformation("In One-shot mode, press ctrl+c or wait for the transfer to finish");

                    while (true)
                    {
                        bool stopProgram = false;
                        try
                        {
                            if (!waitingForMessage.Wait(idleTime, combinedSource.Token))
                                stopProgram = true;
                        }
                        catch (OperationCanceledException)
                        {
                            stopProgram = true;
                        }

                        if (stopProgram)
                        {
                            // Program aborted or No message arrived, stop reading
                            _finished = true;

                            internalSource.Cancel();

                            _channel.BasicCancel(consumer.ConsumerTag);
                            break;
                        }

                        // Message arrived, wait for it to be sent to target
                        messageSent.WaitOne();
                    }
                }
                else
                    throw new InvalidOperationException();
            }
        }

        private static MessageItem Convert(BasicDeliverEventArgs item)
        {
            Dictionary<string, object> properties = null;

            if (item.BasicProperties.Headers != null)
            {
                properties = new Dictionary<string, object>();

                foreach ((string key, object value) in item.BasicProperties.Headers)
                    properties.Add(key, value);
            }

            MessageItem data = new AmqpMessageItem
            {
                Data = item.Body,
                Exchange = item.Exchange,
                RoutingKey = item.RoutingKey,
                Properties = properties,
                DeliveryTag = item.DeliveryTag
            };

            if (item.BasicProperties.IsTimestampPresent())
                data.Created = DateTimeOffset.FromUnixTimeSeconds(item.BasicProperties.Timestamp.UnixTime).UtcDateTime;
            return data;
        }

        public void Acknowledge(ICollection<MessageItem> items)
        {
            int added = 0, target = items.Count - 1;
            foreach (AmqpMessageItem item in items)
            {
                bool isLast = ++added == target;

                _channel.BasicAck(item.DeliveryTag, !isLast);
            }
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _channel?.Dispose();
        }

        class AmqpMessageItem : MessageItem
        {
            internal ulong DeliveryTag { get; set; }
        }
    }
}