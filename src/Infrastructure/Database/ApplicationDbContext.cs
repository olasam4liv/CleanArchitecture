using System.Security.Claims;
using System.Text.Json;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities;
using Domain.Todos;
using Domain.Users;
using Infrastructure.AuditLog;
using Infrastructure.DomainEvents;
using Infrastructure.Outbox;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SharedKernel;
using SharedKernel.Helper.Interfaces;

namespace Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventsDispatcher domainEventsDispatcher,
    IEnumerable<IIntegrationEventMapper> integrationEventMappers,
    IHttpContextAccessor httpContextAccessor,
    ISerializerService _serializer)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<TodoItem> TodoItems { get; set; }
    public DbSet<AuditTrailEntity> AuditTrails { get; set; }

    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.HasDefaultSchema(Schemas.Default);

        // Apply global query filter for soft delete
        modelBuilder.AppendGlobalQueryFilter<ISoftDelete>(e => e.DeletedOn == null);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        string userId = httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false ? httpContextAccessor?.HttpContext?.User.FindFirstValue("Id") ?? null : string.Empty;
        // Extract and clear domain events before saving, so we can
        // persist Outbox messages in the same transaction (transactional outbox).
        List<IDomainEvent> domainEvents = ExtractDomainEvents();

        if (domainEvents.Count > 0)
        {
            foreach (IDomainEvent domainEvent in domainEvents)
            {
                // Try to map domain event to integration event
                IntegrationEvent? integrationEvent = MapToIntegrationEvent(domainEvent);

                var message = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    OccurredOnUtc = DateTime.UtcNow,
                    Type = integrationEvent?.GetType().FullName ?? domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                    Content = JsonSerializer.Serialize(
                        integrationEvent ?? (object)domainEvent,
                        integrationEvent?.GetType() ?? domainEvent.GetType())
                };

                OutboxMessages.Add(message);
            }
        }

        List<AuditTrail> auditEntries = HandleAuditingBeforeSaveChanges(userId);
        int result = await base.SaveChangesAsync(cancellationToken);
        await HandleAuditingAfterSaveChangesAsync(auditEntries, cancellationToken);

        // In-process handlers still execute after commit to avoid blocking the transaction.
        if (domainEvents.Count > 0)
        {
            await domainEventsDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return result;
    }

#region Audit Log 
    private List<AuditTrail> HandleAuditingBeforeSaveChanges(string? userId)
    {
        string remoteIpAddress = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;

        foreach (EntityEntry entry in ChangeTracker.Entries<IAuditableEntity>().ToList())
        {
            var auditableEntity = (IAuditableEntity)entry.Entity;
            
            switch (entry.State)
            {
                case EntityState.Added:
                    auditableEntity.CreatedBy = userId;
                    auditableEntity.CreatedAt = DateTime.UtcNow;
                    auditableEntity.LastModifiedBy = userId;
                    auditableEntity.RemoteIpAddress = remoteIpAddress;
                    break;

                case EntityState.Modified:
                    auditableEntity.UpdatedAt = DateTime.UtcNow;
                    auditableEntity.LastModifiedBy = userId;
                    auditableEntity.RemoteIpAddress = remoteIpAddress;
                    break;

                case EntityState.Deleted:
                    if (entry.Entity is ISoftDelete softDelete)
                    {
                        softDelete.DeletedBy = userId;
                        softDelete.DeletedOn = DateTime.UtcNow;
                        entry.State = EntityState.Modified;
                        auditableEntity.RemoteIpAddress = remoteIpAddress;
                    }

                    break;
            }
        }

        ChangeTracker.DetectChanges();

        var trailEntries = new List<AuditTrail>();
        foreach (EntityEntry entry in ChangeTracker.Entries<IAuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Deleted or EntityState.Modified)
            .ToList())
        {
            var trailEntry = new AuditTrail(entry, _serializer)
            {
                TableName = entry.Entity.GetType().Name,
                UserId = userId ?? "System",
                RemoteIpAddress = remoteIpAddress
            };
            trailEntries.Add(trailEntry);
            foreach (PropertyEntry property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    trailEntry.TemporaryProperties.Add(property);
                    continue;
                }

                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    trailEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        trailEntry.TrailType = TrailType.Create;
                        trailEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        trailEntry.TrailType = TrailType.Delete;
                        trailEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified && entry.Entity is ISoftDelete && property.OriginalValue == null && property.CurrentValue != null)
                        {
                            trailEntry.ChangedColumns.Add(propertyName);
                            trailEntry.TrailType = TrailType.Delete;
                            trailEntry.OldValues[propertyName] = property.OriginalValue;
                            trailEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        else if (property.IsModified && property.OriginalValue?.Equals(property.CurrentValue) == false)
                        {
                            trailEntry.ChangedColumns.Add(propertyName);
                            trailEntry.TrailType = TrailType.Update;
                            trailEntry.OldValues[propertyName] = property.OriginalValue;
                            trailEntry.NewValues[propertyName] = property.CurrentValue;
                        }

                        break;
                }
            }
        }

        foreach (AuditTrail auditEntry in trailEntries.Where(e => !e.HasTemporaryProperties))
        {
            AuditTrailEntity auditEntryEntity = auditEntry.ToAuditTrail();
            //auditEntryEntity.OldValues = ServiceHelper.MaskSensitiveInfo(auditEntryEntity.OldValues);
            //auditEntryEntity.NewValues = ServiceHelper.MaskSensitiveInfo(auditEntryEntity.NewValues);
            AuditTrails.Add(auditEntryEntity);
        }

        return trailEntries.Where(e => e.HasTemporaryProperties).ToList();
    }

    private Task HandleAuditingAfterSaveChangesAsync(List<AuditTrail> trailEntries, CancellationToken cancellationToken = new())
    {
        if (trailEntries == null || trailEntries.Count == 0)
        {
            return Task.CompletedTask;
        }

        foreach (AuditTrail entry in trailEntries)
        {
            foreach (PropertyEntry prop in entry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    entry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                else
                {
                    entry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                }
            }
            AuditTrailEntity auditEntryEntity = entry.ToAuditTrail();
            //auditEntryEntity.OldValues = ServiceHelper.MaskSensitiveInfo(auditEntryEntity?.OldValues);
            //auditEntryEntity.NewValues = ServiceHelper.MaskSensitiveInfo(auditEntryEntity?.NewValues);
            AuditTrails.Add(auditEntryEntity);
        }
        return SaveChangesAsync(cancellationToken);
    }
    #endregion

    private List<IDomainEvent> ExtractDomainEvents()
    {
        return ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();
                return events;
            })
            .ToList();
    }

    private IntegrationEvent? MapToIntegrationEvent(IDomainEvent domainEvent)
    {
        foreach (IIntegrationEventMapper mapper in integrationEventMappers)
        {
            IntegrationEvent? integrationEvent = mapper.Map(domainEvent);
            if (integrationEvent != null)
            {
                return integrationEvent;
            }
        }

        return null;
    }
}
