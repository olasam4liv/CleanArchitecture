using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Todos;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Helper;
using SharedKernel.Model.Responses;

namespace Application.Todos.Copy;

internal sealed class CopyTodoCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext)
    : ICommandHandler<CopyTodoCommand, Guid>
{
    public async Task<ResponseModel<Guid>> Handle(CopyTodoCommand command, CancellationToken cancellationToken)
    {
        if (userContext.UserId != command.UserId)
        {
            return ResponseModel<Guid>.Failure(
                MessageReader.GetMessage(ResponseStatusCode.Unauthorized.Value, "en"),
                ResponseStatusCode.Unauthorized.ResponseCode);
        }

        User? user = await context.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return ResponseModel<Guid>.Failure(
                MessageReader.GetMessage(ResponseStatusCode.UserNotFound.Value, "en"),
                ResponseStatusCode.UserNotFound.ResponseCode);
        }

        TodoItem? existingTodo = await context.TodoItems.AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == command.TodoId && t.UserId == command.UserId, cancellationToken);

        if (existingTodo is null)
        {
            return ResponseModel<Guid>.Failure(
                MessageReader.GetMessage(ResponseStatusCode.ResourceNotFoundError.Value, "en"),
                ResponseStatusCode.ResourceNotFoundError.ResponseCode);
        }

        var copiedTodoItem = new TodoItem
        {
            UserId = user.Id,
            Description = existingTodo.Description,
            Priority = existingTodo.Priority,
            DueDate = existingTodo.DueDate,
            Labels = existingTodo.Labels.ToList(),
            IsCompleted = false,
            CreatedAt = dateTimeProvider.UtcNow
        };

        copiedTodoItem.RaiseDomainEvent(new TodoItemCreatedDomainEvent(copiedTodoItem.Id));

        context.TodoItems.Add(copiedTodoItem);

        await context.SaveChangesAsync(cancellationToken);

        return ResponseModel<Guid>.Success(
            copiedTodoItem.Id,
            MessageReader.GetMessage(ResponseStatusCode.Successful.Value, "en"));
    }
}
