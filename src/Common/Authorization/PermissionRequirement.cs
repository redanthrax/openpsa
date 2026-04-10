using Microsoft.AspNetCore.Authorization;

namespace Common.Authorization;

public class PermissionRequirement : IAuthorizationRequirement {
    public string PermissionKey { get; }

    public PermissionRequirement(string permissionKey) {
        PermissionKey = permissionKey;
    }
}
