using Common.Authorization;
using Common.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OpenPsa.Modules.Security.Authorization;

namespace OpenPsa.Modules.Security;

public class SecurityModule : IModule {
    public void ConfigureServices(IServiceCollection services) {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
    }
}
