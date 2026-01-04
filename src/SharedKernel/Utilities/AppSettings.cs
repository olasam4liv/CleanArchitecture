using System;
using System.Collections.Generic;
using System.Text;

namespace SharedKernel.Helper;

public class AppSettings
{
    public bool AddBufferSize { get; set; }
    public bool EncryptResponseData { get; set; }
    public int EncryptResponseDataBufferSize { get; set; }
    public bool IsLive { get; set; }
    public bool UseEncryptedData { get; set; }
    public EmailServiceSettings EmailServiceSettings { get; set; } = new EmailServiceSettings();
    
    /// <summary>
    /// Base URL for email activation links (e.g., https://yourdomain.com or http://localhost:5000)
    /// </summary>
    public string EmailActivationBaseUrl { get; set; } = string.Empty;
}
    

/// <summary>
/// Configuration options for the external email service.
/// </summary>
public sealed class EmailServiceSettings
{
    public const string SectionName = "EmailService";

    /// <summary>
    /// Base URL of the email service API.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Endpoint path for sending simple emails.
    /// </summary>
    public string SendEmail { get; set; } = string.Empty;

    /// <summary>
    /// Endpoint path for sending emails with attachments.
    /// </summary>
    public string SendEmailWithAttachment { get; set; } = string.Empty;

    /// <summary>
    /// API key for authentication with the email service.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
