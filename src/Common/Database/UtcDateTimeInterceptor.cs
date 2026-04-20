using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Common.Database;

public class UtcDateTimeInterceptor : SaveChangesInterceptor {
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default) {
        NormalizeDateTimes(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result) {
        NormalizeDateTimes(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private static void NormalizeDateTimes(DbContext? context) {
        if (context is null) return;

        foreach (var entry in context.ChangeTracker.Entries()) {
            if (entry.State is not (EntityState.Added or EntityState.Modified)) continue;

            foreach (var property in entry.Properties) {
                if (property.CurrentValue is DateTime dt && dt.Kind != DateTimeKind.Utc) {
                    property.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                }
            }
        }
    }
}
