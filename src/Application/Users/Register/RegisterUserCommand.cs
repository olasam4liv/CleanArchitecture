using Application.Abstractions.Messaging;
using SharedKernel.Model.Responses;

namespace Application.Users.Register;

public sealed record RegisterUserCommand(string Email, string FirstName, string LastName, string Password)
    : IResponseCommand<Guid>;
