using Domain.BaseEntities;

namespace Domain.Entities;

public sealed class ApiClient : AuditableEntity
{
    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ClientKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string Iv { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
