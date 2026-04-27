using Common.Authorization;
using Common.Modules;

namespace OpenPsa.Modules.Notes;

public class NotesModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        // Notes only expose list/create/delete endpoints today.
        registry.RegisterCrudPermissions("notes", "Notes", "Notes",
            CrudVerbs.List | CrudVerbs.Create | CrudVerbs.Delete);
    }
}
