using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.DomainEvents.Todo;
using Domain.Entities;
using Domain.Todos;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Helper;
using SharedKernel.Model.Responses;

namespace Application.Todos.Create;

internal sealed class CreateTodoCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext)
    : ICommandHandler<CreateTodoCommand, Guid>
{
    public async Task<ResponseModel<Guid>> Handle(CreateTodoCommand command, CancellationToken cancellationToken)
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

        var todoItem = new TodoItem
        {
            UserId = user.Id,
            Description = command.Description,
            Priority = command.Priority,
            DueDate = command.DueDate,
            Labels = command.Labels,
            IsCompleted = false,
            CreatedAt = dateTimeProvider.UtcNow
        };

        todoItem.RaiseDomainEvent(new TodoItemCreatedDomainEvent(todoItem.Id));

        context.TodoItems.Add(todoItem);

        await context.SaveChangesAsync(cancellationToken);

        return ResponseModel<Guid>.Success(
            todoItem.Id,
            MessageReader.GetMessage(ResponseStatusCode.Successful.Value, "en"));
    }
}
