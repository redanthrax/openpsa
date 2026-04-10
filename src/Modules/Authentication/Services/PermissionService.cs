using Common.Database;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Authentication.Models;

namespace OpenPsa.Modules.Authentication.Services;

public class PermissionService : IPermissionService {
    private readonly OpenPsaDbContext _dbContext;

    public PermissionService(OpenPsaDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default) {
        var user = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken)
            .ConfigureAwait(false);

        if (user == null) return [];

        if (user.IsSuperAdmin) {
            return await _dbContext.Set<Permission>()
                .Select(p => p.Key)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        if (user.RoleIds.Count == 0) return [];

        var roles = await _dbContext.Set<Role>()
            .Where(r => user.RoleIds.Contains(r.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return roles.SelectMany(r => r.PermissionKeys).Distinct();
    }

    public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionKey, CancellationToken cancellationToken = default) {
        var permissions = await GetUserPermissionsAsync(userId, cancellationToken).ConfigureAwait(false);
        return permissions.Contains(permissionKey);
    }
}
