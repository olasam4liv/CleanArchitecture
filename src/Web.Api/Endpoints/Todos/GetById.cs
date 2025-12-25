using Application.Abstractions.Messaging;
using Application.Todos.GetById;
using SharedKernel.Model.Responses;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Todos;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("todos/{id:guid}", async (
            Guid id,
            IQueryHandler<GetTodoByIdQuery, TodoResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new GetTodoByIdQuery(id);

            ResponseModel<TodoResponse> result = await handler.Handle(command, cancellationToken);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithTags(Tags.Todos)
        .RequireAuthorization();
    }
}
