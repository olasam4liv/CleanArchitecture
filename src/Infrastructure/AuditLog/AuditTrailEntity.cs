using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.AuditLog;

public class AuditTrailEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? UserId { get; set; }
    public string? RemoteIpAddress { get; set; }
    public string? Type { get; set; }
    public string? TableName { get; set; }
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedColumns { get; set; }
    public string? PrimaryKey { get; set; }
}
