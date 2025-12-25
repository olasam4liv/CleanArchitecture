using Application.Abstractions.Messaging;
using Application.Todos.Copy;
using SharedKernel.Model.Responses;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Todos;

internal sealed class Copy : IEndpoint
{
    public sealed class Request
    {
        public Guid UserId { get; set; }
        public Guid TodoId { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("todos/{todoId}/copy", async (
            Guid todoId,
            Request request,
            ICommandHandler<CopyTodoCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CopyTodoCommand
            {
                UserId = request.UserId,
                TodoId = todoId
            };

            ResponseModel<Guid> result = await handler.Handle(command, cancellationToken);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithTags(Tags.Todos)
        .RequireAuthorization();
    }
}
