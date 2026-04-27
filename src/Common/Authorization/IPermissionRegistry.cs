namespace Common.Authorization;

public interface IPermissionRegistry {
    void RegisterPermission(string key, string name, string description, string category);
    IEnumerable<PermissionDefinition> GetAll();
}

public record PermissionDefinition(string Key, string Name, string Description, string Category);

[Flags]
public enum CrudVerbs {
    None   = 0,
    List   = 1 << 0,
    View   = 1 << 1,
    Create = 1 << 2,
    Update = 1 << 3,
    Delete = 1 << 4,
    All    = List | View | Create | Update | Delete,
}

public static class PermissionRegistryExtensions {
    /// <summary>
    /// Register a subset of CRUD permissions for an entity. Default is All.
    /// Pass explicit verbs when an entity does not expose every endpoint, so the
    /// permission registry stays in sync with the actual API surface.
    /// </summary>
    public static void RegisterCrudPermissions(this IPermissionRegistry registry,
        string prefix, string entityName, string category, CrudVerbs verbs = CrudVerbs.All) {
        var lower = entityName.ToLowerInvariant();
        if (verbs.HasFlag(CrudVerbs.List))
            registry.RegisterPermission($"{prefix}.list", $"List {entityName}", $"View all {lower} in the system", category);
        if (verbs.HasFlag(CrudVerbs.View))
            registry.RegisterPermission($"{prefix}.view", $"View {entityName}", $"View details of a specific {lower}", category);
        if (verbs.HasFlag(CrudVerbs.Create))
            registry.RegisterPermission($"{prefix}.create", $"Create {entityName}", $"Add new {lower} to the system", category);
        if (verbs.HasFlag(CrudVerbs.Update))
            registry.RegisterPermission($"{prefix}.update", $"Update {entityName}", $"Modify {lower} information", category);
        if (verbs.HasFlag(CrudVerbs.Delete))
            registry.RegisterPermission($"{prefix}.delete", $"Delete {entityName}", $"Remove {lower} from the system", category);
    }
}
