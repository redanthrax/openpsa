namespace Contracts.Roles;

public class UpdateRoleRequest {
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = [];
}
