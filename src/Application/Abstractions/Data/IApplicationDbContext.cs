using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<TodoItem> TodoItems { get; }
    DbSet<ApiClient> ApiClients { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
