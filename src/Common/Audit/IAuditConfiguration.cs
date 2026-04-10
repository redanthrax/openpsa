namespace Common.Audit;

public interface IAuditConfiguration {
    bool ShouldAudit(string entityName);
    IEnumerable<string> GetExcludedProperties(string entityName);
}

public class DefaultAuditConfiguration : IAuditConfiguration {
    private static readonly HashSet<string> ExcludedEntities = ["AuditEntry"];
    private static readonly HashSet<string> ExcludedProperties = ["PasswordHash", "PasswordSalt", "RefreshToken"];

    public bool ShouldAudit(string entityName) => !ExcludedEntities.Contains(entityName);

    public IEnumerable<string> GetExcludedProperties(string entityName) => ExcludedProperties;
}
