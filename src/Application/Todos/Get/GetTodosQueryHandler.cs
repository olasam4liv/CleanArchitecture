using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Helper;
using SharedKernel.Model.Responses;

namespace Application.Todos.Get;

internal sealed class GetTodosQueryHandler(IApplicationDbContext context, IUserContext userContext)
    : IQueryHandler<GetTodosQuery, List<TodoResponse>>
{
    public async Task<ResponseModel<List<TodoResponse>>> Handle(GetTodosQuery query, CancellationToken cancellationToken)
    {
        if (query.UserId != userContext.UserId)
        {
            return ResponseModel<List<TodoResponse>>.Failure(
                MessageReader.GetMessage(ResponseStatusCode.Unauthorized.Value, "en"),
                ResponseStatusCode.Unauthorized.ResponseCode);
        }

        List<TodoResponse> todos = await context.TodoItems
            .Where(todoItem => todoItem.UserId == query.UserId)
            .Select(todoItem => new TodoResponse
            {
                Id = todoItem.Id,
                UserId = todoItem.UserId,
                Description = todoItem.Description,
                DueDate = todoItem.DueDate,
                Labels = todoItem.Labels,
                IsCompleted = todoItem.IsCompleted,
                CreatedAt = todoItem.CreatedAt,
                CompletedAt = todoItem.CompletedAt
            })
            .ToListAsync(cancellationToken);

        return ResponseModel<List<TodoResponse>>.Success(
            todos,
            MessageReader.GetMessage(ResponseStatusCode.Successful.Value, "en"));
    }
}
