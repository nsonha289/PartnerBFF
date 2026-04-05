namespace PartnerBFF.Application.Interfaces
{
    public interface IMessagePublisherBroker
    {
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default);
    }
}
