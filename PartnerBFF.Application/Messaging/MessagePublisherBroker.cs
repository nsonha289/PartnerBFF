using PartnerBFF.Application.Exceptions;
using PartnerBFF.Application.Interfaces;
using PartnerBFF.Application.Models;
using PartnerBFF.Infrastructure.Interfaces;
using PartnerBFF.Infrastructure.Messaging.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerBFF.Application.Messaging
{
    public class MessagePublisherBroker : IMessagePublisherBroker
    {
        private readonly IEnumerable<IMessagePublisher> _publishers;

        public MessagePublisherBroker(IEnumerable<IMessagePublisher> publishers)
        {
            _publishers = publishers;
        }

        public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        {
            var publisher = message switch
            {
                TransactionMessage => _publishers.OfType<RabbitMqPublisher>().First(),
                _ => throw new MessagePublishException("No publisher for this type")
            };

            try
            {
                await publisher.PublishAsync(message, cancellationToken);
            }
            catch (Exception ex) 
            {
                throw new MessagePublishException(ex.Message);
            }
            
        }
    }
}
