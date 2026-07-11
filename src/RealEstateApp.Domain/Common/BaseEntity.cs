namespace RealEstateApp.Domain.Common;

public abstract class BaseEntity<TId> : IAuditableEntity
{
    public TId Id { get; protected set; } = default!;
    public DateTimeOffset CreatedAt { get; protected set; } = DomainTime.UtcNow;
    public DateTimeOffset? UpdatedAt { get; protected set; }

    public void Touch() => UpdatedAt = DomainTime.UtcNow;
}

public abstract class BaseEntity : BaseEntity<int>;
