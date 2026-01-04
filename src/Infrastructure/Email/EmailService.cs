using Application.Abstractions.Email;
using Infrastructure.Email.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Helper;
using SharedKernel.Model.Responses;
using SharedKernel.Utilities.Interfaces;

namespace Infrastructure.Email;

/// <summary>
/// Email service implementation using external email API and HttpClientHelper.
/// </summary>
internal sealed class EmailService : IEmailService
{
    private readonly AppSettings _appSettings;
    private readonly IHttpClientHelper _httpClientHelper;
    private readonly ILogger<EmailService> _logger;
    private readonly Dictionary<string, string> _headers;

    public EmailService(
        IOptions<AppSettings> appSettings,
        IHttpClientHelper httpClientHelper,
        ILogger<EmailService> logger)
    {
        _appSettings = appSettings.Value;
        _httpClientHelper = httpClientHelper;
        _logger = logger;
        _headers = new Dictionary<string, string> { { "X-API-Key", _appSettings.EmailServiceSettings.ApiKey } };
    }

    public async Task<bool> SendEmailAsync(
        SendEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        string url = $"{_appSettings.EmailServiceSettings.BaseUrl.TrimEnd('/')}/{_appSettings.EmailServiceSettings.SendEmail.TrimStart('/')}";

        ResponseModel<EmailServiceResponse> response = await _httpClientHelper.MakeAPIRequestAsync<EmailServiceResponse>(
            url,
            HttpMethod.Post,
            request,
            _headers);

        if (response.IsSuccess)
        {
            _logger.LogInformation("Email sent successfully to {To} with subject '{Subject}'", request.To, request.Subject);
            return true;
        }

        _logger.LogError("Failed to send email to {To}. Error: {Error}", request.To, response.Message);
        return false;
    }

    public async Task<bool> SendEmailWithAttachmentAsync(
        SendEmailWithAttachmentRequest request,
        CancellationToken cancellationToken = default)
    {
        string url = $"{_appSettings.EmailServiceSettings.BaseUrl.TrimEnd('/')}/{_appSettings.EmailServiceSettings.SendEmailWithAttachment.TrimStart('/')}";

        ResponseModel<EmailServiceResponse> response = await _httpClientHelper.MakeAPIRequestAsync<EmailServiceResponse>(
            url,
            HttpMethod.Post,
            request,
            _headers);

        if (response.IsSuccess)
        {
            _logger.LogInformation(
                "Email with attachment sent successfully to {To} with subject '{Subject}'",
                request.To,
                request.Subject);
            return true;
        }

        _logger.LogError("Failed to send email with attachment to {To}. Error: {Error}", request.To, response.Message);
        return false;
    }
}
