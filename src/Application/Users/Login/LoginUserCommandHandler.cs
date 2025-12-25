using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Helper;
using SharedKernel.Model.Responses;

namespace Application.Users.Login;

internal sealed class LoginUserCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider) : ICommandHandler<LoginUserCommand, string>
{
    public async Task<ResponseModel<string>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null)
        {
            return ResponseModel<string>.Failure(
                MessageReader.GetMessage(ResponseStatusCode.UserNotFound.Value, "en"),
                ResponseStatusCode.UserNotFound.ResponseCode);
        }

        bool verified = passwordHasher.Verify(command.Password, user.PasswordHash);

        if (!verified)
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
