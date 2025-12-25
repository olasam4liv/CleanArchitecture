using Web.Api.Middleware;

namespace Web.Api.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();

        return app;
    }

    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestTimingMiddleware>();

        return app;
    }
}
