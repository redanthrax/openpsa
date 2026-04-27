using Common.Authorization;
using Common.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenPsa.Modules.Authentication.Services;

namespace OpenPsa.Modules.Authentication;

public class AuthenticationModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        // No delete-user endpoint; deactivation is handled via UpdateUser.
        registry.RegisterCrudPermissions("users", "Users", "User Management",
            CrudVerbs.All & ~CrudVerbs.Delete);
        registry.RegisterCrudPermissions("roles", "Roles", "User Management");
        registry.RegisterPermission("permissions.list", "List Permissions", "View all permissions", "User Management");
        registry.RegisterPermission("audit.list", "View Audit Log", "View the audit trail", "User Management");
        registry.RegisterPermission("audit.entity", "View Entity History", "View history of a specific entity", "User Management");
    }

    public void ConfigureServices(IServiceCollection services) {
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddHostedService<PermissionSyncHostedService>();
    }
}
