using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.DomainEvents.Todo;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Helper;
using SharedKernel.Model.Responses;

namespace Application.Todos.Delete;

internal sealed class DeleteTodoCommandHandler(IApplicationDbContext context, IUserContext userContext)
    : ICommandHandler<DeleteTodoCommand>
{
    public async Task<ResponseModel> Handle(DeleteTodoCommand command, CancellationToken cancellationToken)
    {
        TodoItem? todoItem = await context.TodoItems
            .SingleOrDefaultAsync(t => t.Id == command.TodoItemId && t.UserId == userContext.UserId, cancellationToken);

        if (todoItem is null)
        {
            return ResponseModel.Failure(
                MessageReader.GetMessage(ResponseStatusCode.ResourceNotFoundError.Value, "en"),
                ResponseStatusCode.ResourceNotFoundError.ResponseCode);
        }

        context.TodoItems.Remove(todoItem);

        todoItem.RaiseDomainEvent(new TodoItemDeletedDomainEvent(todoItem.Id));

        await context.SaveChangesAsync(cancellationToken);

        return ResponseModel.Success(
            MessageReader.GetMessage(ResponseStatusCode.Successful.Value, "en"));
    }
}
