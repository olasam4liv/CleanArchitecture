using System;
using System.Collections.Generic;
using System.Text;
using Domain.BaseEntities;

namespace Domain.Entities;

public class AppUser: AuditableEntity
{
    public string? Email { get; set; }
    public string Iv { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public string? ClientId { get; set; }
}
