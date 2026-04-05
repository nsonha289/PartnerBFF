using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PartnerBFF.Infrastructure.Configurations;
using PartnerBFF.Infrastructure.Interfaces;
using PartnerBFF.Infrastructure.Policies;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace PartnerBFF.Infrastructure.Messaging.RabbitMQ
{
    public class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqPublisher> _logger;

        public RabbitMqPublisher(
            IRabbitMqConnectionFactory connectionFactory, 
            IOptions<RabbitMqSettings> settings, 
            ILogger<RabbitMqPublisher> logger)
        {
            _settings = settings.Value;
            _connection = connectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            SetupExchangeAndQueue();
            
            _logger = logger;
        }

        public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var body = Encoding.UTF8.GetBytes(json);

                var properties = new BasicProperties
                {
                    Persistent = true,   // message survives broker restart
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString(),
                    CorrelationId = Activity.Current?.Id ?? Guid.NewGuid().ToString(),
                    Timestamp = new AmqpTimestamp(
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                };

                var retryPolicy = RabbitMqRetryPolicy.GetPolicy(_logger);
                await retryPolicy.ExecuteAsync(async () =>
                {
                    await _channel.BasicPublishAsync(
                        exchange: _settings.ExchangeName,
                        routingKey: _settings.RoutingKey,
                        mandatory: true,
                        basicProperties: properties,
                        body: body,
                        cancellationToken: cancellationToken);
                });


                _logger.LogInformation(
                    "Message {MessageId} published to exchange {Exchange} " +
                    "with routing key {RoutingKey}",
                    properties.MessageId,
                    _settings.ExchangeName,
                    _settings.RoutingKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish message to exchange {Exchange}",
                    _settings.ExchangeName);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _channel.CloseAsync();
            await _connection.CloseAsync();
            _channel.Dispose();
            _connection.Dispose();
        }

        private void SetupExchangeAndQueue()
        {
            // 3. Declare exchange — where messages are sent TO
            _channel.ExchangeDeclareAsync(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Direct,
                durable: true,     // survives broker restart
                autoDelete: false
            ).GetAwaiter().GetResult();

            // 4. Declare queue — where messages are stored
            _channel.QueueDeclareAsync(
                queue: _settings.QueueName,
                durable: true,     // survives broker restart
                exclusive: false,
                autoDelete: false
            ).GetAwaiter().GetResult();

            // 5. Bind queue to exchange via routing key
            _channel.QueueBindAsync(
                queue: _settings.QueueName,
                exchange: _settings.ExchangeName,
                routingKey: _settings.RoutingKey
            ).GetAwaiter().GetResult();
        }
    }
}
