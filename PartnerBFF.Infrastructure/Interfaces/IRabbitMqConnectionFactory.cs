using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerBFF.Infrastructure.Interfaces
{
    public interface IRabbitMqConnectionFactory
    {
        Task<IConnection> CreateConnectionAsync();
    }
}
