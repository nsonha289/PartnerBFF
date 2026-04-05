using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace PartnerBFF.Infrastructure.Policies
{
    public static class PartnerVerifierPolicy
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()         // handles 5xx, 408
                .Or<TimeoutRejectedException>()     // handles Polly timeout
                .Or<TimeoutException>()             // handles our mock timeout
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2s, 4s, 8s
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine(
                            $"Retry {retryAttempt} after {timespan.TotalSeconds}s " +
                            $"due to: {outcome.Exception?.Message}");
                    });
        }
    }
}
