using Common.Domain;

namespace OpenPsa.Modules.Authentication.Models;

public class User : BaseEntity {
    public string? LocalPasswordHash { get; set; }
    public string? ExternalProvider { get; set; }
    public string? ExternalSubjectId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsSuperAdmin { get; set; }
    public List<Guid> RoleIds { get; set; } = [];
    public DateTime? LastLoginAt { get; set; }
}
