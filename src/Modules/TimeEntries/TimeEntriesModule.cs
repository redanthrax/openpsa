using Common.Authorization;
using Common.Modules;

namespace OpenPsa.Modules.TimeEntries;

public class TimeEntriesModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        // No get-by-id endpoint for time entries today.
        registry.RegisterCrudPermissions("time-entries", "Time Entries", "Time Tracking",
            CrudVerbs.All & ~CrudVerbs.View);
        registry.RegisterCrudPermissions("rate-cards", "Rate Cards", "Time Tracking");
    }
}
