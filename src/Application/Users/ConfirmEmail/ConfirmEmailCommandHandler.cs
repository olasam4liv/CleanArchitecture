using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using SharedKernel.Helper;
using SharedKernel.Model.Responses;

namespace Application.Users.ConfirmEmail;

internal sealed class ConfirmEmailCommandHandler(IIdentityService identityService)
    : ICommandHandler<ConfirmEmailCommand, bool>
{
    public async Task<ResponseModel<bool>> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        User? user = await identityService.FindByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return ResponseModel<bool>.Failure(
                MessageReader.GetMessage(ResponseStatusCode.UserNotFound.Value, "en"),
                ResponseStatusCode.UserNotFound.ResponseCode);
        }

        if (user.EmailConfirmed)
        {
            return ResponseModel<bool>.Failure(
                "Email already confirmed",
                ResponseStatusCode.ValidationError.ResponseCode);
        }

        // Decode the token (it was URL-encoded when sent in the link)
        string decodedToken = Uri.UnescapeDataString(command.Token);

        IdentityResult result = await identityService.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
        {
            string errors = string.Join("; ", result.Errors.Select(e => e.Description));
            
            return ResponseModel<bool>.Failure(
                $"Email confirmation failed: {errors}",
                ResponseStatusCode.ValidationError.ResponseCode);
        }

        return ResponseModel<bool>.Success(
            true,
            "Email confirmed successfully. You can now log in.");
    }
}
