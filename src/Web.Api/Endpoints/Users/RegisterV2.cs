using Application.Abstractions.Messaging;
using Application.Users.Register;
using SharedKernel.Model.Responses;
using Web.Api.Extensions;
using Web.Api.Infrastructure;
using Asp.Versioning;

namespace Web.Api.Endpoints.Users;

/// <summary>
/// Maps the POST users/register endpoint for API v2 with enhanced response.
/// </summary>
internal sealed class RegisterV2 : IEndpoint
{
    /// <summary>
    /// Response includes additional user information for v2.
    /// </summary>
    public sealed record Response(Guid UserId, string Email, string FullName);

    /// <summary>
    /// Registers the v2 endpoint with extended response information.
    /// </summary>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/register", async (
            RegisterUserCommand request,
            IResponseCommandHandler<RegisterUserCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            ResponseModel<Guid> result = await handler.Handle(request, cancellationToken);

            if (result.IsSuccess)
            {
                // V2 returns enhanced response with user details
                Response v2Response = new(
                    result.Data!,
                    request.Email,
                    $"{request.FirstName} {request.LastName}");
                
                return Results.Ok(ResponseModel<Response>.Success(v2Response, result.Message));
            }

            return Results.BadRequest(result);
        })
        .AllowAnonymous()
        .MapToApiVersion(new ApiVersion(2, 0))
        .WithTags(Tags.Users)
        .Produces<ResponseModel<Response>>(StatusCodes.Status200OK)
        .Produces<ResponseModel<Guid>>(StatusCodes.Status400BadRequest)
        .WithOpenApi(operation => new OpenApiOperation(operation)
        {
            Summary = "Register a new user (v2 - Enhanced)",
            Description = "Creates a new user and returns a ResponseModel containing enhanced user information including userId, email, and full name on success or validation errors on failure. This is the v2 endpoint with improved response structure."
        });
    }
}
