using System.Linq;
using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Users.GetById;
using Domain.DomainEvents.User;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using SharedKernel;
using SharedKernel.Helper;
using SharedKernel.Model.Responses;

namespace Application.Users.Register;

internal sealed class RegisterUserCommandHandler(IIdentityService identityService)
    : IResponseCommandHandler<RegisterUserCommand, UserResponse>
{
    public async Task<ResponseModel<UserResponse>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await identityService.EmailExistsAsync(command.Email, cancellationToken))
        {
            return ResponseModel<UserResponse>.Failure(
                new UserResponse(),
                MessageReader.GetMessage(ResponseStatusCode.EmailAlreadyExist.Value, "en"),
                ResponseStatusCode.EmailAlreadyExist.ResponseCode);
        }

        var user = new User
        {
            Email = command.Email,
            UserName = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            LockoutEnabled = true
        };

        user.RaiseDomainEvent(new UserRegisteredDomainEvent(user.Id));

        IdentityResult result = await identityService.CreateUserAsync(user, command.Password, cancellationToken);

        if (!result.Succeeded)
        {
            string errors = string.Join("; ", result.Errors.Select(e => e.Description));

            return ResponseModel<UserResponse>.Failure(
                new UserResponse(),
                errors,
                ResponseStatusCode.ValidationError.ResponseCode);
        }

        var response = new UserResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName
        };

        return ResponseModel<UserResponse>.Success(
            response,
            MessageReader.GetMessage(ResponseStatusCode.Successful.Value, "en"));
    }
}
