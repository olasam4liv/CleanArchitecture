using System;
using System.Collections.Generic;
using System.Text;
using SharedKernel.Helper.Interfaces;

namespace Domain.BaseEntities;

public abstract class AuditableEntity : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }
    public string? RemoteIpAddress { get; set; }

    protected AuditableEntity()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
