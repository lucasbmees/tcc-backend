namespace TccSharkTank.Domain.Entities;

public abstract class EntityBase<TKey>
{
    public required TKey Id { get; init; }
}

public abstract class AuditableEntityBase<TKey> : EntityBase<TKey>
{
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
}

