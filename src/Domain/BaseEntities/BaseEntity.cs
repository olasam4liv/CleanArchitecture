using SharedKernel;

namespace Domain.BaseEntities;

public abstract class BaseEntity : Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
