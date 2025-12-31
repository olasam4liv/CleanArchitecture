using System.Net;
using Application.Abstractions.Authentication;
using Application.Authentication.Clients;
using Domain.BaseEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using SharedKernel.Utilities;

namespace Web.Api.Filters;

public class DecryptRequestDataFilter<T>(
    IOptions<AppSettings> settings,
    ILogger<DecryptRequestDataFilter<T>> logger,    
    IHttpContextAccessor httpContext,
    IAuthService authService
        ) : IAsyncActionFilter
{
    private readonly AppSettings _settings = settings.Value;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContext;
    private readonly ILogger<DecryptRequestDataFilter<T>> _logger = logger;
    private readonly IAuthService _authService = authService;

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
          
            ResponseModel<GetApiClientResponse> client = await _authService.GetClientByKeyAsync(extractedClientId.ToString(), CancellationToken.None);
            if (!client.IsSuccess || client.Data == null ||
                string.IsNullOrWhiteSpace(client.Data.ClientKey) ||
                string.IsNullOrWhiteSpace(client.Data.ClientIv))
            {
                await SetForbiddenResponseAsync("Invalid Client Id");
                return;
            }
            if (string.IsNullOrWhiteSpace(client.Data.SecretKey))
            {
                await SetForbiddenResponseAsync("Invalid Encryption Key");
                return;
            }

            T deserializedData = await ServiceHelper.DecryptRequest<T>(request.EncryptedData!, client.Data.SecretKey, client.Data.ClientIv);
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
