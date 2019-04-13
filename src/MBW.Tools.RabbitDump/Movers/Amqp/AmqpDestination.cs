using System;
using System.Collections.Generic;
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
        private IBasicPublishBatch _batch;
        private int _batchCount;

        public AmqpDestination(ArgumentsModel model, ILogger<AmqpDestination> logger)
        {
            _model = model;
            _logger = logger;

            UriConnectionFactory connectionFactory = new UriConnectionFactory(new Uri(model.Output));
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _batch = _channel.CreateBasicPublishBatch();
        }

        public void WriteData(MessageItem item)
        {
            string exchange = _model.Exchange ?? item.Exchange;
            string routingKey = _model.RoutingKey ?? item.RoutingKey;

            BasicProperties basicProperties = new BasicProperties
            {
                Headers = new Dictionary<string, object>()
            };

            if (item.Created.HasValue)
                basicProperties.Timestamp = new AmqpTimestamp(((DateTimeOffset)item.Created).ToUnixTimeSeconds());

            if (item.Properties != null)
            {
                foreach ((string key, object value) in item.Properties)
                    basicProperties.Headers.Add(key, value);
            }

            _batch.Add(exchange, routingKey, false, basicProperties, item.Data);
            _batchCount++;

            if (_batchCount >= _model.BatchSize)
            {
                _logger.LogDebug("Publishing batch of {Count} messages to destination", _batchCount);

                _batch.Publish();
                _batch = _channel.CreateBasicPublishBatch();
                _batchCount = 0;
            }
        }

        public void Dispose()
        {
            if (_batchCount > 0)
            {
                _logger.LogDebug("Publishing batch of {Count} messages to destination", _batchCount);

                _batch.Publish();
            }

            _batchCount = 0;
            _batch = null;

            _channel.Dispose();
            _connection.Dispose();
        }
    }
}