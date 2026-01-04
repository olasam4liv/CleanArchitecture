namespace Application.Abstractions.Email;

/// <summary>
/// Email service interface for sending emails via external service.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a simple email.
    /// </summary>
    Task<bool> SendEmailAsync(
        SendEmailRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email with attachments.
    /// </summary>
    Task<bool> SendEmailWithAttachmentAsync(
        SendEmailWithAttachmentRequest request,
        CancellationToken cancellationToken = default);
}
