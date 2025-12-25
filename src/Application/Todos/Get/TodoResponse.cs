namespace Application.Todos.Get;

public sealed class TodoResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public ICollection<string> Labels { get; set; } = [];
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
