using Ardalis.SmartEnum;

namespace SharedKernel.Model.Responses;

public sealed class ResponseStatusCode : SmartEnum<ResponseStatusCode, string>
{
    public string ResponseCode { get; set; }

    public ResponseStatusCode(string name, string value, string responseCode) : base(name, value)
    {
        ResponseCode = responseCode;
    }

    // Success statuses
    public static readonly ResponseStatusCode Successful = new(nameof(Successful), "Successful", "00");
    public static readonly ResponseStatusCode Processing = new(nameof(Processing), "Processing", "01");
    public static readonly ResponseStatusCode Failed = new(nameof(Failed), "Failed", "02");

    // Domain-specific error statuses (03-09)
    public static readonly ResponseStatusCode ValidationError = new(nameof(ValidationError), "ValidationError", "03");
    public static readonly ResponseStatusCode EmailAlreadyExist = new(nameof(EmailAlreadyExist), "EmailAlreadyExist", "04");
    public static readonly ResponseStatusCode PhoneNumberAlreadyExist = new(nameof(PhoneNumberAlreadyExist), "PhoneNumberAlreadyExist", "05");
    public static readonly ResponseStatusCode InvalidCredentials = new(nameof(InvalidCredentials), "InvalidCredentials", "06");
    public static readonly ResponseStatusCode UserNotFound = new(nameof(UserNotFound), "UserNotFound", "07");
    public static readonly ResponseStatusCode UserInactive = new(nameof(UserInactive), "UserInactive", "08");

    // HTTP standard error statuses (4xx)
    public static readonly ResponseStatusCode BadRequest = new(nameof(BadRequest), "BadRequest", "400");
    public static readonly ResponseStatusCode Unauthorized = new(nameof(Unauthorized), "Unauthorized", "401");
    public static readonly ResponseStatusCode ProviderAuthorizationError = new(nameof(ProviderAuthorizationError), "ProviderAuthorizationError", "401");
    public static readonly ResponseStatusCode Forbidden = new(nameof(Forbidden), "Forbidden", "403");
    public static readonly ResponseStatusCode ResourceNotFoundError = new(nameof(ResourceNotFoundError), "ResourceNotFoundError", "404");
    public static readonly ResponseStatusCode Conflict = new(nameof(Conflict), "Conflict", "409");

    // HTTP standard error statuses (5xx)
    public static readonly ResponseStatusCode InternalServerError = new(nameof(InternalServerError), "InternalServerError", "500");
    public static readonly ResponseStatusCode ServiceUnavailable = new(nameof(ServiceUnavailable), "ServiceUnavailable", "503");
}