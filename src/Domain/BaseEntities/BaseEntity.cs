using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.BaseEntities;

public abstract class BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
}
