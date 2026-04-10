using Common.Authorization;
using Common.Modules;

namespace OpenPsa.Modules.Settings;

public class SettingsModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        registry.RegisterPermission("settings.view", "View Settings", "View system settings", "Settings");
        registry.RegisterPermission("settings.update", "Update Settings", "Update system settings", "Settings");
    }
}
