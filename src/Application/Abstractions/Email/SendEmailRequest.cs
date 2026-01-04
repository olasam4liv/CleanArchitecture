namespace Application.Abstractions.Email;

/// <summary>
/// Request payload for sending a simple email.
/// </summary>
public sealed record SendEmailRequest(
    string To,
    string Subject,
    string Body,
    string? Cc = null,
    string? Bcc = null);
