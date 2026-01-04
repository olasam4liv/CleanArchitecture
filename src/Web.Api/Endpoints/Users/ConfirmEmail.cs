using Application.Users.ConfirmEmail;
using SharedKernel.Model.Responses;

namespace Web.Api.Endpoints.Users;

/// <summary>
/// Endpoint for confirming user email addresses.
/// </summary>
internal sealed class ConfirmEmailEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the GET endpoint for email confirmation.
    /// Users click this link from their email.
    /// </summary>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/confirm-email", async (
            Guid userId,
            string token,
            ICommandHandler<ConfirmEmailCommand, bool> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ConfirmEmailCommand(userId, token);
            ResponseModel<bool> result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                // Return a user-friendly HTML page or redirect to frontend
                return Results.Content(
                    @"<!DOCTYPE html>
                    <html>
                    <head>
                        <title>Email Confirmed</title>
                        <style>
                            body { font-family: Arial, sans-serif; text-align: center; padding: 50px; }
                            .success { color: #28a745; font-size: 24px; }
                            .message { margin-top: 20px; }
                        </style>
                    </head>
                    <body>
                        <div class='success'>✓ Email Confirmed Successfully!</div>
                        <div class='message'>Your email has been verified. You can now log in to your account.</div>
                        <div class='message'><a href='/'>Go to Login</a></div>
                    </body>
                    </html>",
                    "text/html");
            }

            // Return error HTML page
            return Results.Content(
                $@"<!DOCTYPE html>
                <html>
                <head>
                    <title>Email Confirmation Failed</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; }}
                        .error {{ color: #dc3545; font-size: 24px; }}
                        .message {{ margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class='error'>✗ Email Confirmation Failed</div>
                    <div class='message'>{result.Message}</div>
                    <div class='message'>The link may have expired or been used already.</div>
                </body>
                </html>",
                "text/html");
        })
        .AllowAnonymous()
        .WithTags("Users")
        .WithName("ConfirmEmail")
        .Produces(200, contentType: "text/html")
        .Produces(400, contentType: "text/html")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Confirms user email address";
            operation.Description = "Verifies the email confirmation token sent to the user's email. This endpoint is typically accessed by clicking the link in the confirmation email.";
            return operation;
        });
    }
}
