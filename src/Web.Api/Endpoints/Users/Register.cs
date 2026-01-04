using Asp.Versioning;
using Application.Users.GetById;

namespace Web.Api.Endpoints.Users;

/// <summary>
/// Maps the POST users/register endpoint to create a new user account.
/// </summary>
internal sealed class Register : IEndpoint
{
    /// <summary>
    /// Registration payload containing user identity and password.
    /// </summary>
    ///public sealed record Request(string Email, string FirstName, string LastName, string Password);

    /// <summary>
    /// Registers the endpoint and returns 200 with a ResponseModel on success or 400 on failure.
    /// </summary>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/register", async (
            RegisterUserCommand request,
            IResponseCommandHandler<RegisterUserCommand, UserResponse> handler,
            CancellationToken cancellationToken) =>
        {
            ResponseModel<UserResponse> result = await handler.Handle(request, cancellationToken);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        })
        .AllowAnonymous()
        .MapToApiVersion(new ApiVersion(1, 0))
        .WithTags(Tags.Users)
        .Produces<ResponseModel<UserResponse>>(StatusCodes.Status200OK)
        .Produces<ResponseModel<UserResponse>>(StatusCodes.Status400BadRequest)
        .WithOpenApi(operation => new OpenApiOperation(operation)
        {
            Summary = "Register a new user",
            Description = "Creates a new user and returns a ResponseModel containing user details on success or validation errors on failure."
        });
    }
}
