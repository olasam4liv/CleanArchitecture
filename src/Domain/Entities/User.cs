using System;
using Microsoft.AspNetCore.Identity;
using SharedKernel;
using SharedKernel.Helper.Interfaces;

namespace Domain.Entities;

public sealed class User : IdentityUser<Guid>, IAuditableEntity, ISoftDelete, IDomainEventEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public User()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedOn { get; set; }

    public string? DeletedBy { get; set; }

    public string? RemoteIpAddress { get; set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
