namespace Common.Authorization;

public interface IPermissionRegistry {
    void RegisterPermission(string key, string name, string description, string category);
    IEnumerable<PermissionDefinition> GetAll();
}

public record PermissionDefinition(string Key, string Name, string Description, string Category);

public static class PermissionRegistryExtensions {
    public static void RegisterCrudPermissions(this IPermissionRegistry registry,
        string prefix, string entityName, string category) {
        registry.RegisterPermission($"{prefix}.list",   $"List {entityName}",   $"View all {entityName.ToLowerInvariant()} in the system",          category);
        registry.RegisterPermission($"{prefix}.view",   $"View {entityName}",   $"View details of a specific {entityName.ToLowerInvariant()}",       category);
        registry.RegisterPermission($"{prefix}.create", $"Create {entityName}", $"Add new {entityName.ToLowerInvariant()} to the system",            category);
        registry.RegisterPermission($"{prefix}.update", $"Update {entityName}", $"Modify {entityName.ToLowerInvariant()} information",               category);
        registry.RegisterPermission($"{prefix}.delete", $"Delete {entityName}", $"Remove {entityName.ToLowerInvariant()} from the system",           category);
    }
}
