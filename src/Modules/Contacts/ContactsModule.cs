using Common.Authorization;
using Common.Modules;

namespace OpenPsa.Modules.Contacts;

public class ContactsModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        registry.RegisterCrudPermissions("contacts", "Contacts", "Contacts");
    }
}
