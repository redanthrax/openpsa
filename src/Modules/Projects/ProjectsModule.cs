using Common.Authorization;
using Common.Modules;

namespace OpenPsa.Modules.Projects;

public class ProjectsModule : IModule {
    public void RegisterPermissions(IPermissionRegistry registry) {
        registry.RegisterCrudPermissions("projects", "Projects", "Projects");
    }
}
