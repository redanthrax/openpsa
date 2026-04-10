namespace Common.Authorization;

public class PermissionRegistry : IPermissionRegistry {
    private readonly List<PermissionDefinition> _permissions = [];

    public void RegisterPermission(string key, string name, string description, string category) {
        if (_permissions.Any(p => p.Key == key)) return;
        _permissions.Add(new PermissionDefinition(key, name, description, category));
    }

    public IEnumerable<PermissionDefinition> GetAll() => _permissions.AsReadOnly();
}
