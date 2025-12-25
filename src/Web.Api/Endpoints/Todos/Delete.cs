using Application.Abstractions.Messaging;
using Application.Todos.Delete;
using SharedKernel.Model.Responses;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Todos;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("todos/{id:guid}", async (
            Guid id,
            ICommandHandler<DeleteTodoCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteTodoCommand(id);

            ResponseModel result = await handler.Handle(command, cancellationToken);

            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result);
        })
        .WithTags(Tags.Todos)
        .RequireAuthorization();
    }
}
