using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Web.Api.Infrastructure.Options;

namespace Web.Api.Middleware;

public class RequestTimingMiddleware(
    RequestDelegate next,
    ILogger<RequestTimingMiddleware> logger,
    IOptions<RequestTimingOptions> options)
{
    private readonly int _thresholdMilliseconds = options.Value.ThresholdMilliseconds;

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await next(context);

        stopwatch.Stop();

        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (elapsedMilliseconds >= _thresholdMilliseconds)
        {
            string method = context.Request.Method;
            string path = context.Request.Path;
            int statusCode = context.Response.StatusCode;

            logger.LogWarning(
                "Slow request {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms",
                method,
                path,
                statusCode,
                elapsedMilliseconds);
        }
    }
}
