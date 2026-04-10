using Common.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace OpenPsa.Modules.Security.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement) {
        if (context.User.HasPermission(requirement.PermissionKey)) {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
