using Common.Authorization;
using Common.Modules;

namespace OpenPsa.Modules.Tickets;

public class TicketsModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        registry.RegisterCrudPermissions("tickets", "Tickets", "Tickets");
    }
}
