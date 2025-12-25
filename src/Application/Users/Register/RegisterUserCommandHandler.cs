using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Helper;
using SharedKernel.Model.Responses;

namespace Application.Users.Register;

internal sealed class RegisterUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    : IResponseCommandHandler<RegisterUserCommand, Guid>
{
    public async Task<ResponseModel<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Email == command.Email, cancellationToken))
        {
            return ResponseModel<Guid>.Failure(
                Guid.Empty,
                MessageReader.GetMessage(ResponseStatusCode.EmailAlreadyExist.Value, "en"),
                ResponseStatusCode.EmailAlreadyExist.ResponseCode);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            PasswordHash = passwordHasher.Hash(command.Password)
        };

        user.RaiseDomainEvent(new UserRegisteredDomainEvent(user.Id));

        context.Users.Add(user);

        await context.SaveChangesAsync(cancellationToken);

        return ResponseModel<Guid>.Success(
            user.Id,
            MessageReader.GetMessage(ResponseStatusCode.Successful.Value, "en"));
    }
}
