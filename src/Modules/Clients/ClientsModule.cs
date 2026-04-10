using Common.Authorization;
using Common.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace OpenPsa.Modules.Clients;

public class ClientsModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        registry.RegisterCrudPermissions("clients", "Clients", "Clients");
    }
}
