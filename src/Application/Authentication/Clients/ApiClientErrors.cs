using SharedKernel;

namespace Application.Authentication.Clients;

public static class ApiClientErrors
{
    public static readonly Error RequestCancelled = Error.Failure("ApiClients.RequestCancelled", "Request was cancelled.");

    public static readonly Error InvalidEmail = Error.Failure("ApiClients.InvalidEmail", "Invalid email format.");

    public static readonly Error InvalidName = Error.Failure("ApiClients.InvalidName", "Name must be at least 3 characters long.");

    public static readonly Error AlreadyExists = Error.Conflict("ApiClients.AlreadyExists", "Client already exists.");

    public static readonly Error InvalidCredentials = Error.Failure("ApiClients.InvalidCredentials", "Invalid client credentials.");
}
