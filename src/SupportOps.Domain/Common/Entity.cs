using System;
using System.Collections.Generic;
using System.Text;

namespace SupportOps.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; }

    public DateTime CreatedAtUtc { get; protected set; }

    public DateTime? UpdatedAtUtc { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAtUtc = DateTime.UtcNow;
    }

    protected void MarkAsUpdated()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}