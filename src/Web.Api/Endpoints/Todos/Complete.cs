using Application.Abstractions.Messaging;
using Application.Todos.Complete;
using SharedKernel.Model.Responses;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Todos;

internal sealed class Complete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("todos/{id:guid}/complete", async (
            Guid id,
            ICommandHandler<CompleteTodoCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CompleteTodoCommand(id);

            ResponseModel result = await handler.Handle(command, cancellationToken);

            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result);
        })
        .WithTags(Tags.Todos)
        .RequireAuthorization();
    }
}
