using Common.Authorization;
using Common.Modules;

namespace OpenPsa.Modules.TimeEntries;

public class TimeEntriesModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        registry.RegisterCrudPermissions("time-entries", "Time Entries", "Time Tracking");
    }
}
