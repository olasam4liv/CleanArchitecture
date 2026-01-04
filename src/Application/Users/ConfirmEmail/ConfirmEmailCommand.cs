using Application.Abstractions.Messaging;

namespace Application.Users.ConfirmEmail;

public sealed record ConfirmEmailCommand(Guid UserId, string Token) : ICommand<bool>;
