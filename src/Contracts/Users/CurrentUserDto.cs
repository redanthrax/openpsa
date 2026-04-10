namespace Contracts.Users;

public class CurrentUserDto {
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsSuperAdmin { get; set; }
    public List<string> Permissions { get; set; } = [];
    public List<string> Roles { get; set; } = [];
}
