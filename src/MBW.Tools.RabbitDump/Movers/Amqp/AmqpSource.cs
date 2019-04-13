using System;
using System.Collections.Generic;
using MBW.Tools.RabbitDump.Options;
using RabbitMQ.Client;

namespace MBW.Tools.RabbitDump.Movers.Amqp
{
    class AmqpSource : ISource
    {
        private readonly ArgumentsModel _model;
        private readonly UriConnectionFactory _connectionFactory;

        public AmqpSource(ArgumentsModel model)
        {
            _model = model;
            _connectionFactory = new UriConnectionFactory(new Uri(model.Input));
        }

        public IEnumerable<MessageItem> GetData()
        {
            using (IConnection conn = _connectionFactory.CreateConnection())
            using (IModel model = conn.CreateModel())
            {
                model.BasicQos(0, 1000, true);

                foreach (string queue in _model.Arguments)
                {
                    BasicGetResult item;

                    do
                    {
                        item = model.BasicGet(queue, true);

                        if (item != null)
                        {
                            Dictionary<string, object> properties = null;

                            if (item.BasicProperties.Headers != null)
                            {
                                properties = new Dictionary<string, object>();

                                foreach ((string key, object value) in item.BasicProperties.Headers)
                                    properties.Add(key, value);
                            }

                            MessageItem data = new MessageItem
                            {
                                Data = item.Body,
                                Exchange = item.Exchange,
                                RoutingKey = item.RoutingKey,
                                Properties = properties
                            };

                            if (item.BasicProperties.IsTimestampPresent())
                                data.Created = DateTimeOffset.FromUnixTimeSeconds(item.BasicProperties.Timestamp.UnixTime).UtcDateTime;

                            yield return data;
                        }
                    } while (item != null);
                }
            }
        }
    }
}