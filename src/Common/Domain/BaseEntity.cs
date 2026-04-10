namespace Common.Domain;

public abstract class BaseEntity<TKey> {
    public TKey Id { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public abstract class BaseEntity : BaseEntity<Guid> {
    protected BaseEntity() => Id = Guid.NewGuid();
}
