namespace Application.Abstractions.Email;

/// <summary>
/// Request payload for sending email with attachments.
/// </summary>
public sealed record SendEmailWithAttachmentRequest(
    string To,
    string Subject,
    string Body,
    string FileExtension,
    IEnumerable<string> Attachment,
    string DocName,
    string? Cc = null,
    string? Bcc = null);
