using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Todos;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Helper;
using SharedKernel.Model.Responses;

namespace Application.Todos.Update;

internal sealed class UpdateTodoCommandHandler(
    IApplicationDbContext context)
    : ICommandHandler<UpdateTodoCommand>
{
    public async Task<ResponseModel> Handle(UpdateTodoCommand command, CancellationToken cancellationToken)
    {
        TodoItem? todoItem = await context.TodoItems
            .SingleOrDefaultAsync(t => t.Id == command.TodoItemId, cancellationToken);

        if (todoItem is null)
        {
            return ResponseModel.Failure(
                MessageReader.GetMessage(ResponseStatusCode.ResourceNotFoundError.Value, "en"),
                ResponseStatusCode.ResourceNotFoundError.ResponseCode);
        }

        todoItem.Description = command.Description;

        await context.SaveChangesAsync(cancellationToken);
        
        return ResponseModel.Success(
            MessageReader.GetMessage(ResponseStatusCode.Successful.Value, "en"));
    }
}
