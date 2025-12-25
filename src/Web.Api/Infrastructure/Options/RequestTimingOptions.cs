namespace Web.Api.Infrastructure.Options;

public sealed class RequestTimingOptions
{
    public int ThresholdMilliseconds { get; set; } = 500;
}
