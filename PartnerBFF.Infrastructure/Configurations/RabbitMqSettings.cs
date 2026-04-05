using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerBFF.Infrastructure.Configurations
{
    public class RabbitMqSettings
    {
        public string Host { get; set; }
        public int Port { get; set; } = 5672;
        public string Username { get; set; }
        public string Password { get; set; }
        public string ExchangeName { get; set; }
        public string QueueName { get; set; }
        public string RoutingKey { get; set; }
    }
}
