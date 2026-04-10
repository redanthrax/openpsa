namespace Contracts.Users;

public class UserDto {
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsSuperAdmin { get; set; }
    public List<string> Roles { get; set; } = [];
}
