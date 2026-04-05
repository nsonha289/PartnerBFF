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

        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,  // open after 3 failures
                    durationOfBreak: TimeSpan.FromSeconds(30), // stay open for 30s
                    onBreak: (outcome, timespan) =>
                    {
                        Console.WriteLine(
                            $"Circuit OPEN for {timespan.TotalSeconds}s " +
                            $"due to: {outcome.Exception?.Message}");
                    },
                    onReset: () => Console.WriteLine("Circuit CLOSED — resuming"),
                    onHalfOpen: () => Console.WriteLine("Circuit HALF-OPEN — testing")
                );
        }

        // Combine both into one pipeline
        public static IAsyncPolicy<HttpResponseMessage> GetResiliencePolicy()
        {
            return Policy.WrapAsync(
                GetRetryPolicy(),
                GetCircuitBreakerPolicy()
            );
        }
    }
}
