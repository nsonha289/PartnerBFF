using Microsoft.Extensions.Options;
using PartnerBFF.Infrastructure.Configurations;
using PartnerBFF.Infrastructure.Interfaces;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace PartnerBFF.Infrastructure.Messaging.RabbitMQ
{
    public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
    {
        private readonly RabbitMqSettings _settings;

        public RabbitMqConnectionFactory(IOptions<RabbitMqSettings> rabbitMqSettings)
        {
            _settings = rabbitMqSettings.Value;
        }

        public async Task<IConnection> CreateConnectionAsync()
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password
            };

            return await factory.CreateConnectionAsync();
        }
    }
}
