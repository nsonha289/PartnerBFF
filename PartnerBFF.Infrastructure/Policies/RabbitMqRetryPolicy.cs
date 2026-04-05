using Microsoft.Extensions.Logging;
using Polly;

namespace PartnerBFF.Infrastructure.Policies
{
    public static class RabbitMqRetryPolicy
    {
        public static IAsyncPolicy GetPolicy(ILogger logger)
        {
            return Policy
            .Handle<Exception>()           
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, attempt)), // 2s, 4s, 8s
                onRetry: (exception, timespan, attempt, context) =>
                {
                    logger.LogWarning(
                        "RabbitMQ publish attempt {Attempt} failed: {Message}. " +
                        "Retrying in {Seconds}s",
                        attempt,
                        exception.Message,
                        timespan.TotalSeconds);
                });
        }
    }
}
