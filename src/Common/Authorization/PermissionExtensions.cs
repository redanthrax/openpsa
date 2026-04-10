using System.Security.Claims;
using Microsoft.AspNetCore.Builder;

namespace Common.Authorization;

public static class PermissionExtensions {
    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder,
        string permissionKey) {
        return builder.RequireAuthorization(policy =>
            policy.AddRequirements(new PermissionRequirement(permissionKey)));
    }

    public static bool HasPermission(this ClaimsPrincipal user, string permissionKey) {
        if (user.FindFirst("is_super_admin")?.Value == "True") return true;
        var permissionsClaim = user.FindFirst("permissions")?.Value;
        if (string.IsNullOrEmpty(permissionsClaim)) return false;
        return permissionsClaim.Split(',').Contains(permissionKey);
    }
}
