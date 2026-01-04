using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using SharedKernel.Helper;
using SharedKernel.Model.Responses;

namespace Application.Users.Login;

internal sealed class LoginUserCommandHandler(
    IIdentityService identityService,
    ITokenProvider tokenProvider) : ICommandHandler<LoginUserCommand, string>
{
    public async Task<ResponseModel<string>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await identityService.FindByEmailAsync(command.Email, cancellationToken);

        if (user is null)
        {
            return ResponseModel<string>.Failure(
                MessageReader.GetMessage(ResponseStatusCode.UserNotFound.Value, "en"),
                ResponseStatusCode.UserNotFound.ResponseCode);
        }

        // Optional: Enforce email confirmation before allowing login
        // Uncomment the following block if you set options.SignIn.RequireConfirmedEmail = true
        // if (!user.EmailConfirmed)
        // {
        //     return ResponseModel<string>.Failure(
        //         "Please confirm your email before logging in. Check your inbox for the confirmation link.",
        //         ResponseStatusCode.ValidationError.ResponseCode);
        // }

        SignInResult result = await identityService.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            return ResponseModel<string>.Failure(
                "Account locked due to repeated failed login attempts. Please try again later.",
                ResponseStatusCode.InvalidCredentials.ResponseCode);
        }

        if (!result.Succeeded)
        {
            return ResponseModel<string>.Failure(
                MessageReader.GetMessage(ResponseStatusCode.InvalidCredentials.Value, "en"),
                ResponseStatusCode.InvalidCredentials.ResponseCode);
        }

        string token = tokenProvider.Create(user);

        return ResponseModel<string>.Success(
            token,
            MessageReader.GetMessage(ResponseStatusCode.Successful.Value, "en"));
    }
}
