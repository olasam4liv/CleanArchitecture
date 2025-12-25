using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Todos;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Helper;
using SharedKernel.Model.Responses;

namespace Application.Todos.Complete;

internal sealed class CompleteTodoCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext)
    : ICommandHandler<CompleteTodoCommand>
{
    public async Task<ResponseModel> Handle(CompleteTodoCommand command, CancellationToken cancellationToken)
    {
        TodoItem? todoItem = await context.TodoItems
            .SingleOrDefaultAsync(t => t.Id == command.TodoItemId && t.UserId == userContext.UserId, cancellationToken);

        if (todoItem is null)
        {
            return ResponseModel.Failure(
                MessageReader.GetMessage(ResponseStatusCode.ResourceNotFoundError.Value, "en"),
                ResponseStatusCode.ResourceNotFoundError.ResponseCode);
        }

        if (todoItem.IsCompleted)
        {
            return ResponseModel.Failure(
                "Todo item is already completed",
                ResponseStatusCode.Conflict.ResponseCode);
        }

        todoItem.IsCompleted = true;
        todoItem.CompletedAt = dateTimeProvider.UtcNow;

        todoItem.RaiseDomainEvent(new TodoItemCompletedDomainEvent(todoItem.Id));

        await context.SaveChangesAsync(cancellationToken);

        return ResponseModel.Success(
            MessageReader.GetMessage(ResponseStatusCode.Successful.Value, "en"));
    }
}
