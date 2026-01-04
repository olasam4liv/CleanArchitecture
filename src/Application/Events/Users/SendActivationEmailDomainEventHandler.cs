using Application.Abstractions.Authentication;
using Application.Abstractions.Email;
using Domain.DomainEvents.User;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;
using SharedKernel.Helper;

namespace Application.Events.Users;

/// <summary>
/// Handles the UserRegistered domain event by sending an activation email.
/// </summary>
internal sealed class SendActivationEmailDomainEventHandler(
    IIdentityService identityService,
    IEmailService emailService,
    IOptions<AppSettings> appSettings,
    ILogger<SendActivationEmailDomainEventHandler> logger) 
    : IDomainEventProcessor<UserRegisteredDomainEvent>
{
    private readonly AppSettings _appSettings = appSettings.Value;

    public async Task Handle(UserRegisteredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            User? user = await identityService.FindByIdAsync(domainEvent.UserId, cancellationToken);
            
            if (user is null)
            {
                logger.LogWarning("User {UserId} not found for activation email", domainEvent.UserId);
                return;
            }

            // Generate secure email confirmation token using Identity
            string token = await identityService.GenerateEmailConfirmationTokenAsync(user);
            
            // URL-encode the token for safe transmission in URL
            string encodedToken = Uri.EscapeDataString(token);
            
            // Build activation link using configured base URL
            string activationLink = $"{_appSettings.EmailActivationBaseUrl.TrimEnd('/')}/api/v1/users/confirm-email?userId={user.Id}&token={encodedToken}";

            string emailBody = EmailTemplates.GetActivationEmailBody(domainEvent.FirstName, activationLink);
            string subject = EmailTemplates.GetActivationEmailSubject();

            var request = new SendEmailRequest(
                domainEvent.Email,
                subject,
                emailBody);

            bool emailSent = await emailService.SendEmailAsync(request, cancellationToken);

            if (emailSent)
            {
                logger.LogInformation("Activation email sent successfully to {Email} for user {UserId}", 
                    domainEvent.Email, domainEvent.UserId);
            }
            else
            {
                logger.LogWarning("Failed to send activation email to {Email} for user {UserId}", 
                    domainEvent.Email, domainEvent.UserId);
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail - email is a side effect, not critical to registration
            logger.LogError(ex, "Error sending activation email to {Email} for user {UserId}", 
                domainEvent.Email, domainEvent.UserId);
        }
    }
}
