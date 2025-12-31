using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Authentication;
using Application.Authentication.Clients;
using Microsoft.Extensions.Primitives;
using SharedKernel.Utilities;

namespace Web.Api.Middleware;

public class DataEncryptionMiddleware(
    RequestDelegate next,
    IConfiguration configuration,
    IOptions<AppSettings> settings,
    ILogger<DataEncryptionMiddleware> logger,
    IAuthService authService)
{
    private readonly RequestDelegate _next = next;
    private readonly AppSettings _settings = settings.Value;
    private readonly ILogger<DataEncryptionMiddleware> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IAuthService _authService = authService;
    private const string ClientId = "ClientId";

    public async Task Invoke(HttpContext context)
    {
        if (!_settings.EncryptResponseData)
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        #region Response body interceptor

        Stream originalBodyStream = context.Response.Body;
        try
        {
            if (!context.Request.Headers.TryGetValue(ClientId, out StringValues extractedClientId))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("Invalid Client Id");
                return;
            }


            string? connectionStr = _configuration.GetConnectionString("Database");

            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;
            await _next(context);

            memoryStream.Seek(0, SeekOrigin.Begin);

            string responseBodyText;
            using (var reader = new StreamReader(memoryStream))
            {
                responseBodyText = await reader.ReadToEndAsync();
            }

            long currPos = memoryStream.Position;

            if (string.IsNullOrWhiteSpace(extractedClientId.ToString()) || string.IsNullOrWhiteSpace(connectionStr))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("Invalid Client Id or Connection String");
                return;
            }
            ResponseModel<GetApiClientResponse> client = await _authService.GetClientByKeyAsync(extractedClientId.ToString()!, CancellationToken.None);
            if (!client.IsSuccess || client.Data == null || 
                string.IsNullOrWhiteSpace(client.Data.ClientKey) || 
                string.IsNullOrWhiteSpace(client.Data.ClientIv) )
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("Invalid Client Key");
                return;
            }

            string? encryptedData = ServiceHelper.AesJsonEncryption(responseBodyText, client.Data.SecretKey, client.Data.ClientIv);

            if (string.IsNullOrWhiteSpace(encryptedData))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("Invalid Encryption Key");
                return;
            }

            string jsonData = JsonSerializer.Serialize(new { Data = encryptedData });
            _logger.LogInformation("ResponseBody: {EncryptedData}", encryptedData);

            context.Response.ContentType = "application/json; charset=UTF-8";
            context.Response.ContentLength = jsonData.Length;

            // Use StreamWriter to write the encrypted data to the response stream
            await using (var writer = new StreamWriter(originalBodyStream, new UTF8Encoding(false), leaveOpen: true))
            {
                await writer.WriteAsync(jsonData);
                await writer.FlushAsync();
            }

            memoryStream.Position = currPos;

            if (_settings.AddBufferSize)
            {
                await memoryStream.CopyToAsync(originalBodyStream, _settings.EncryptResponseDataBufferSize);
            }
            else
            {
                await memoryStream.CopyToAsync(originalBodyStream);
            }
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }

        await originalBodyStream.DisposeAsync();

        #endregion

        stopwatch.Stop();
        double elapsedTime = stopwatch.ElapsedMilliseconds;
        _logger.LogInformation("EncryptResponseData Elapsed Time: {ElapsedTimeMs} ms", elapsedTime);
    }
}
