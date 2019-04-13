using System;
using RabbitMQ.Client;

namespace MBW.Tools.RabbitDump.Movers.Amqp
{
    class UriConnectionFactory
    {
        private IConnectionFactory _connectionFactoryImplementation;

        public UriConnectionFactory(Uri uri)
        {
            string[] userSplits = uri.UserInfo?.Split(':', 2);
            string user = null, password = null;
            if (userSplits != null && userSplits.Length == 2)
            {
                user = userSplits[0];
                password = userSplits[1];
            }

            _connectionFactoryImplementation = new ConnectionFactory
            {
                HostName = uri.Host,
                Port = uri.IsDefaultPort ? 5672 : uri.Port,
                UserName = user,
                Password = password,
                VirtualHost = uri.AbsolutePath,
                AutomaticRecoveryEnabled = true
            };
        }

        public IConnection CreateConnection()
        {
            return _connectionFactoryImplementation.CreateConnection();
        }
    }
}