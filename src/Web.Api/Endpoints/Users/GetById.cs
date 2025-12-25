using Application.Abstractions.Messaging;
using Application.Users.GetById;
using SharedKernel.Model.Responses;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{userId}", async (
            Guid userId,
            IQueryHandler<GetUserByIdQuery, UserResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetUserByIdQuery(userId);

            ResponseModel<UserResponse> result = await handler.Handle(query, cancellationToken);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        })
        .HasPermission(Permissions.UsersAccess)
        .WithTags(Tags.Users);
    }
}
