using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using SharedKernel.Helper;
using SharedKernel.Helper.Interfaces;

namespace SharedKernel;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedKernel(this IServiceCollection services)
    {   
        services.AddSingleton<ISerializerService, SerializerService>();

        // Named client with resilience policies
       services.AddHttpClient("HttpClientHelper")
                .AddPolicyHandler((sp, _) => GetRetryPolicy(sp.GetRequiredService<ILoggerFactory>().CreateLogger("HttpClientResilience")))
                .AddPolicyHandler((sp, _) => GetCircuitBreakerPolicy(sp.GetRequiredService<ILoggerFactory>().CreateLogger("HttpClientResilience")));

        return services;
    }

    private static Polly.Retry.AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    logger.LogWarning(
                        "Retry {RetryCount} after {Delay}s due to {StatusCode}",
                        retryCount,
                        timespan.TotalSeconds,
                        outcome.Result?.StatusCode ?? System.Net.HttpStatusCode.InternalServerError);
                });
    }

    private static Polly.CircuitBreaker.AsyncCircuitBreakerPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, breakDelay) =>
                {
                    logger.LogWarning(
                        "Circuit opened for {BreakDelay}s due to {StatusCode}",
                        breakDelay.TotalSeconds,
                        outcome.Result?.StatusCode ?? System.Net.HttpStatusCode.InternalServerError);
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit reset");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Circuit half-open: trial call allowed");
                });
    }
}
