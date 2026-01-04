namespace Application.Events.Users;

/// <summary>
/// Helper to build email activation templates.
/// </summary>
public static class EmailTemplates
{
    private const string ActivationSubject = "Activate Your Account";

    /// <summary>
    /// Creates an activation email body with a verification link.
    /// </summary>
    public static string GetActivationEmailBody(string firstName, string activationLink)
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 20px; border: 1px solid #ddd; border-radius: 0 0 5px 5px; }}
        .button {{ display: inline-block; background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #666; text-align: center; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Welcome to Clean Architecture</h1>
        </div>
        <div class=""content"">
            <p>Hello {firstName},</p>
            <p>Thank you for registering with us! To activate your account, please click the button below:</p>
            <a href=""{activationLink}"" class=""button"">Activate Account</a>
            <p>This link will expire in 24 hours. If you did not create this account, please ignore this email.</p>
            <p>Best regards,<br/>The Clean Architecture Team</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2026 Clean Architecture. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Gets the subject for activation email.
    /// </summary>
    public static string GetActivationEmailSubject()
    {
        return ActivationSubject;
    }
}
