using Common.Authorization;
using Common.Modules;
using Microsoft.Extensions.DependencyInjection;
using OpenPsa.Modules.Sla.Services;

namespace OpenPsa.Modules.Sla;

public class SlaModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        registry.RegisterCrudPermissions("sla-policies", "SLA Policies", "SLA");
        registry.RegisterPermission("sla.view-instances", "View SLA Instances", "View SLA tracking on tickets", "SLA");
    }

    public void ConfigureServices(IServiceCollection services) {
        services.AddHostedService<SlaBreachCheckerService>();
    }
}
