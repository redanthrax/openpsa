using Common.Authorization;
using Common.Modules;

namespace OpenPsa.Modules.Notes;

public class NotesModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        registry.RegisterCrudPermissions("notes", "Notes", "Notes");
    }
}
