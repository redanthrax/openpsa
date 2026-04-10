using Common.Domain;

namespace OpenPsa.Modules.Authentication.Models;

public class Role : BaseEntity {
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> PermissionKeys { get; set; } = [];
}
