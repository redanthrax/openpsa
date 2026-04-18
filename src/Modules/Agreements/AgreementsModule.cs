using Common.Authorization;
using Common.Modules;

namespace OpenPsa.Modules.Agreements;

public class AgreementsModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        registry.RegisterCrudPermissions("agreements", "Agreements", "Agreements");
    }
}
