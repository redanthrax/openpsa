using Contracts.Users;

namespace OpenPsa.Web.Features.Authentication.Services;

public interface IUserPermissionService {
    bool IsInitialized { get; }
    bool IsSuperAdmin { get; }
    CurrentUserDto? CurrentUser { get; }
    bool HasPermission(string permission);
    bool HasAnyPermission(params string[] permissions);
    bool HasAllPermissions(params string[] permissions);
    Task InitializeAsync();
    void Reset();
    event Action? OnPermissionsChanged;
}
