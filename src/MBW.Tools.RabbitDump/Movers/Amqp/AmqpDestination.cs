using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using MBW.Tools.RabbitDump.Options;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace MBW.Tools.RabbitDump.Movers.Amqp
{
    class AmqpDestination : IDestination, IDisposable
    {
        private readonly ArgumentsModel _model;
        private readonly ILogger<AmqpDestination> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public AmqpDestination(ArgumentsModel model, ILogger<AmqpDestination> logger)
        {
            _model = model;
            _logger = logger;

            Uri amqpUri = new Uri(model.Output);
            _logger.LogDebug("Connecting to AMQP for writing at {Uri}", amqpUri);

            UriConnectionFactory connectionFactory = new UriConnectionFactory(amqpUri);

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }

        public (ITargetBlock<MessageItem> writer, IDataflowBlock finalBlock) GetWriter(ISource acknowledgeSource)
        {
            BatchBlock<MessageItem> batcher = new BatchBlock<MessageItem>(_model.BatchSize);
            ActionBlock<MessageItem[]> writer = new ActionBlock<MessageItem[]>(items =>
            {
                IBasicPublishBatch batch = _channel.CreateBasicPublishBatch();

                foreach (MessageItem item in items)
                {
                    string exchange = _model.Exchange ?? item.Exchange;
                    string routingKey = _model.RoutingKey ?? item.RoutingKey;

                    IBasicProperties basicProperties = _channel.CreateBasicProperties();
                    basicProperties.Headers = new Dictionary<string, object>();
                    basicProperties.Persistent = item.Persistent;

                    if (item.Created.HasValue)
                        basicProperties.Timestamp = new AmqpTimestamp(((DateTimeOffset)item.Created).ToUnixTimeSeconds());

                    if (item.Properties != null)
                    {
                        foreach ((string key, object value) in item.Properties)
                            basicProperties.Headers.Add(key, value);
                    }

                    batch.Add(exchange, routingKey, true, basicProperties, item.Data);
                }

                _logger.LogDebug("Writing {Count} messages to AMQP", items.Length);
                batch.Publish();

                acknowledgeSource.Acknowledge(items);
            });

            batcher.LinkTo(writer, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            return (batcher, writer);
        }
    }
}