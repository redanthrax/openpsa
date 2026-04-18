using Common.Authorization;
using Common.Modules;

namespace OpenPsa.Modules.Assets;

public class AssetsModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        registry.RegisterCrudPermissions("assets", "Assets", "Assets");
    }
}
