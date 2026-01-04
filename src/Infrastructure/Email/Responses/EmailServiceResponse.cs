namespace Infrastructure.Email.Responses;

/// <summary>
/// Response from email service API.
/// </summary>
public sealed record EmailServiceResponse(
    bool Success,
    string Message);
