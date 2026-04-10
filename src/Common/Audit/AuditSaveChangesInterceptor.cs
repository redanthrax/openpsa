using System.Text.Json;
using Common.Authentication;
using Common.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Common.Audit;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor {
    private readonly IUserContext _userContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditConfiguration _auditConfiguration;
    private List<AuditEntryData>? _pendingAudits;

    public AuditSaveChangesInterceptor(
        IUserContext userContext,
        IHttpContextAccessor httpContextAccessor,
        IAuditConfiguration auditConfiguration) {
        _userContext = userContext;
        _httpContextAccessor = httpContextAccessor;
        _auditConfiguration = auditConfiguration;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result) {
        if (eventData.Context != null) {
            StampUpdatedAt(eventData.Context);
            _pendingAudits = CollectAuditData(eventData.Context);
        }
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) {
        if (eventData.Context != null) {
            StampUpdatedAt(eventData.Context);
            _pendingAudits = CollectAuditData(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result) {
        if (eventData.Context != null && _pendingAudits?.Count > 0)
            SaveAuditEntries(eventData.Context, _pendingAudits);
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default) {
        if (eventData.Context != null && _pendingAudits?.Count > 0)
            await SaveAuditEntriesAsync(eventData.Context, _pendingAudits, cancellationToken).ConfigureAwait(false);
        return await base.SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    private static void StampUpdatedAt(DbContext context) {
        var now = DateTime.UtcNow;
        foreach (var entry in context.ChangeTracker.Entries()) {
            if (entry.State != EntityState.Modified) continue;
            var updatedAt = entry.Properties.FirstOrDefault(p => p.Metadata.Name == nameof(BaseEntity.UpdatedAt));
            if (updatedAt != null) updatedAt.CurrentValue = now;
        }
    }

    private List<AuditEntryData> CollectAuditData(DbContext context) {
        var audits = new List<AuditEntryData>();
        var httpContext = _httpContextAccessor.HttpContext;

        foreach (var entry in context.ChangeTracker.Entries()) {
            if (entry.State is EntityState.Detached or EntityState.Unchanged) continue;
            var entityName = entry.Entity.GetType().Name;
            if (entityName == nameof(AuditEntry)) continue;
            if (!_auditConfiguration.ShouldAudit(entityName)) continue;

            var auditData = new AuditEntryData {
                Entry = entry,
                EntityName = entityName,
                Action = entry.State switch {
                    EntityState.Added => AuditAction.Created,
                    EntityState.Modified => AuditAction.Updated,
                    EntityState.Deleted => AuditAction.Deleted,
                    _ => AuditAction.Updated
                },
                UserId = _userContext.UserId,
                UserEmail = _userContext.UserEmail,
                UserName = _userContext.UserName,
                IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString()
            };

            CollectPropertyValues(entry, auditData, _auditConfiguration.GetExcludedProperties(entityName));
            audits.Add(auditData);
        }

        return audits;
    }

    private static void CollectPropertyValues(EntityEntry entry, AuditEntryData auditData, IEnumerable<string> excludedProperties) {
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();
        var changedProperties = new List<string>();
        var excludedSet = excludedProperties.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var property in entry.Properties) {
            var propertyName = property.Metadata.Name;
            if (excludedSet.Contains(propertyName)) continue;
            if (property.Metadata.IsShadowProperty()) continue;

            if (property.Metadata.IsPrimaryKey()) {
                auditData.TempKeyProperties.Add(propertyName, property);
            }

            switch (entry.State) {
                case EntityState.Added:
                    newValues[propertyName] = property.CurrentValue;
                    break;
                case EntityState.Deleted:
                    oldValues[propertyName] = property.OriginalValue;
                    break;
                case EntityState.Modified:
                    if (property.IsModified) {
                        oldValues[propertyName] = property.OriginalValue;
                        newValues[propertyName] = property.CurrentValue;
                        changedProperties.Add(propertyName);
                    }
                    break;
            }
        }

        if (oldValues.Count > 0) auditData.OldValues = JsonSerializer.Serialize(oldValues, JsonOptions);
        if (newValues.Count > 0) auditData.NewValues = JsonSerializer.Serialize(newValues, JsonOptions);
        if (changedProperties.Count > 0) auditData.ChangedProperties = JsonSerializer.Serialize(changedProperties, JsonOptions);
    }

    private static void SaveAuditEntries(DbContext context, List<AuditEntryData> list) {
        context.Set<AuditEntry>().AddRange(list.Select(CreateAuditEntry));
        context.SaveChanges();
    }

    private static async Task SaveAuditEntriesAsync(DbContext context, List<AuditEntryData> list, CancellationToken ct) {
        context.Set<AuditEntry>().AddRange(list.Select(CreateAuditEntry));
        await context.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private static AuditEntry CreateAuditEntry(AuditEntryData data) {
        var entityId = data.TempKeyProperties.Count > 0
            ? string.Join(",", data.TempKeyProperties.Values.Select(p => p.CurrentValue?.ToString() ?? "null"))
            : "unknown";

        return new AuditEntry {
            EntityName = data.EntityName,
            EntityId = entityId,
            Action = data.Action,
            OldValues = data.OldValues,
            NewValues = data.NewValues,
            ChangedProperties = data.ChangedProperties,
            UserId = data.UserId,
            UserEmail = data.UserEmail,
            UserName = data.UserName,
            IpAddress = data.IpAddress,
            UserAgent = data.UserAgent,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = new() {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private class AuditEntryData {
        public EntityEntry Entry { get; set; } = null!;
        public string EntityName { get; set; } = string.Empty;
        public AuditAction Action { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? ChangedProperties { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public Dictionary<string, PropertyEntry> TempKeyProperties { get; } = [];
    }
}
