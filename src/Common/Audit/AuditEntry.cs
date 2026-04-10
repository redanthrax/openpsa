using Common.Domain;

namespace Common.Audit;

public class AuditEntry : BaseEntity<int> {
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? ChangedProperties { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public enum AuditAction {
    None = 0,
    Created = 1,
    Updated = 2,
    Deleted = 3
}
