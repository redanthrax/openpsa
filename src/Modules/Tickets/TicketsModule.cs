using Common.Authorization;
using Common.Modules;

namespace OpenPsa.Modules.Tickets;

public class TicketsModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        registry.RegisterCrudPermissions("tickets", "Tickets", "Tickets");
        // Ticket queues do not expose a get-by-id endpoint.
        registry.RegisterCrudPermissions("ticket-queues", "Ticket Queues", "Tickets",
            CrudVerbs.All & ~CrudVerbs.View);
    }
}
