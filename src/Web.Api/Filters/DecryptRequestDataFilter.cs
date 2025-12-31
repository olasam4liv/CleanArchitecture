using System.Net;
using Domain.BaseEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using SharedKernel.Utilities;

namespace Web.Api.Filters;

public class DecryptRequestDataFilter<T>(
    IOptions<AppSettings> settings,
    ILogger<DecryptRequestDataFilter<T>> logger,
    IConfiguration configuration,
    IHttpContextAccessor httpContext
        ) : IAsyncActionFilter
{
    private readonly AppSettings _settings = settings.Value;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContext;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<DecryptRequestDataFilter<T>> _logger = logger;

    private const string CLIENTID = "ClientId";
    private const string REQUEST = "request";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!_settings.UseEncryptedData)
        {
            await next();
            return;
        }

        if (!context.ActionArguments.TryGetValue(REQUEST, out object? output) ||
            output is not BaseEncryptedRequestDto request)
        {
            context.Result = CreateBadRequestResult("Invalid Request Data");
            return;
        }

        //_logger.LogInformation($"{typeof(T).Name} ENCRYPTED REQUEST ==> {ServiceHelper.SerializeAsJson(request)}");
        if (!request.IsValid(out string problemSource))
        {
            context.Result = CreateBadRequestResult($" - {problemSource}");
            return;
        }
        if (!string.IsNullOrWhiteSpace(request.EncryptedData) && !ServiceHelper.IsBase64String(request.EncryptedData))
        {
            context.Result = CreateBadRequestResult($" - {problemSource}");
            return;
        }

        try
        {

            if (_httpContextAccessor.HttpContext is null || !_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(CLIENTID, out StringValues extractedClientId))
            {
                await SetForbiddenResponseAsync("Invalid Client Id");
                return;
            }
            bool isLive = bool.Parse(_configuration.GetSection("AppSettings:isLive").Value ?? "false");
            string? connectionStr = isLive ?
                _configuration.GetSection("DatabaseSettings:ProdConnectionString").Value :
                _configuration.GetSection("DatabaseSettings:DevConnectionString").Value;

            var apiUser = ServiceHelper.GetApiUser(extractedClientId, connectionStr);
            if (string.IsNullOrWhiteSpace(apiUser.SecretKey))
            {
                await SetForbiddenResponseAsync("Invalid Encryption Key");
                return;
            }

            var deserializedData = await ServiceHelper.DecryptRequest<T>(request.EncryptedData, apiUser.SecretKey, apiUser.Iv);
            if (deserializedData is not null)
            {
                context.ActionArguments["request"] = deserializedData;
            }
            await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Decryption error");
            context.Result = CreateBadRequestResult($"Decryption error");
        }
    }
    private ObjectResult CreateBadRequestResult(string message) =>
        new(Error.Failure("BadRequest", message))
        {
            StatusCode = (int)HttpStatusCode.BadRequest
        };

    private async Task SetForbiddenResponseAsync(string message)
    {
        if (_httpContextAccessor.HttpContext is not null)
        {
            _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            _logger.LogError("Decryption Key error: {Message}", message);
            await _httpContextAccessor.HttpContext.Response.WriteAsync(message);
        }
        else
        {
            _logger.LogError("HttpContext is null. Cannot set forbidden response. Message: {Message}", message);
        }
    }
}
