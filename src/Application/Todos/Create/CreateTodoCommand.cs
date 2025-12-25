using Application.Abstractions.Messaging;
using Domain.Todos;

namespace Application.Todos.Create;

public sealed class CreateTodoCommand : ICommand<Guid>
{
    public Guid UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public ICollection<string> Labels { get; set; } = [];
    public Priority Priority { get; set; }
}
public static class CreateTodoCommandExtensions
{
    public static CreateTodoCommand CreateTodo(
        this Guid userId,
        string description,
        DateTime? dueDate,
        ICollection<string> labels,
        Priority priority)
{
    return new CreateTodoCommand
    {
        UserId = userId,
        Description = description,
        DueDate = dueDate,
        Labels = labels,
        Priority = priority
    };
}
}