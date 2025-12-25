using Application;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Web.Api.MaskingOperators;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.log.json", optional: true, reloadOnChange: true);
    
if (builder.Environment.IsDevelopment())
{
    DotNetEnv.Env.Load(); // Loads .env file into process environment variable
    
}

IConfiguration configuration = builder.Configuration.ReplaceEnvironmentVariables();

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
    .WriteTo.Console(formatProvider: System.Globalization.CultureInfo.InvariantCulture)
    .ReadFrom.Configuration(configuration)
    .Enrich.WithSensitiveDataMasking(options =>
    {
        options.MaskingOperators = new List<IMaskingOperator>
        {
            new TotalMaskingOption(configuration),
            new PartialMaskOption(configuration)
        };
    }));
builder.Services.AddSwaggerGenWithAuth();

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(configuration);

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

// Health checks (includes DB readiness)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<Infrastructure.Database.ApplicationDbContext>("database");

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("default", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
        limiterOptions.AutoReplenishment = true;
    });
});

string? otlpEndpoint = configuration["OpenTelemetry:Otlp:Endpoint"];

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
        resource.AddService(serviceName: builder.Environment.ApplicationName,
                            serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0"))
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddRuntimeInstrumentation();
        metrics.AddMeter(
            "Microsoft.AspNetCore.Hosting",
            "Microsoft.AspNetCore.Server.Kestrel",
            "System.Net.Http");
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            metrics.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
        }
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
        }
    });



// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.Configure<RequestTimingOptions>(builder.Configuration.GetSection("RequestTiming"));

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

WebApplication app = builder.Build();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .HasApiVersion(new ApiVersion(2, 0))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder api = app.MapGroup("api/v{version:apiVersion}")
    .WithApiVersionSet(apiVersionSet)
    .AllowAnonymous(); // Allow anonymous by default, endpoints can override with RequireAuthorization

app.MapEndpoints(api);

app.ApplyMigrations();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();
    app.ApplyMigrations();
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
    context.Response.Headers.TryAdd("Content-Security-Policy", "default-src 'self'; frame-ancestors 'none'; object-src 'none'; base-uri 'self';");
    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Liveness/readiness endpoints
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.UseRequestContextLogging();
app.UseRequestTiming();

app.UseSerilogRequestLogging();

app.UseCors("Default");

// Global exception handling to wrap technical failures as ResponseModel with 500
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var problem = SharedKernel.Model.Responses.ResponseModel.Failure(
            message: app.Environment.IsDevelopment() ? ex.Message : "An unexpected error occurred",
            responseCode: SharedKernel.Model.Responses.ResponseStatusCode.InternalServerError.ResponseCode);
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problem);
    }
});

// Correlation ID propagation
app.Use(async (context, next) =>
{
    const string headerName = "X-Correlation-Id";
    if (!context.Request.Headers.TryGetValue(headerName, out Microsoft.Extensions.Primitives.StringValues correlationId) || string.IsNullOrWhiteSpace(correlationId))
    {
        correlationId = Guid.NewGuid().ToString();
        context.Request.Headers[headerName] = correlationId;
    }
    context.Response.Headers[headerName] = correlationId!;
    await next();
});

// Enable rate limiting
app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

// REMARK: If you want to use Controllers, you'll need this.
app.MapControllers();

await app.RunAsync();

// REMARK: Required for functional and integration tests to work.
namespace Web.Api
{
    public partial class Program;
}
