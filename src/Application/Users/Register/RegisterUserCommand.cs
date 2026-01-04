using Application.Abstractions.Messaging;
using Application.Users.GetById;
using SharedKernel.Model.Responses;

namespace Application.Users.Register;

public sealed record RegisterUserCommand(string Email, string FirstName, string LastName, string Password)
    : IResponseCommand<UserResponse>;
