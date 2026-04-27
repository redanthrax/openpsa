using Common.Domain;

namespace OpenPsa.Modules.Authentication.Models;

/// <summary>
/// Persisted record of a refresh token. The raw token value is never stored —
/// only its SHA-256 hash. Rotation is tracked via <see cref="ReplacedByTokenId"/>
/// so an attempted reuse of a rotated token can revoke the entire family.
/// </summary>
public class RefreshToken : BaseEntity {
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }

    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
}
