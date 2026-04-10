namespace OpenPsa.Modules.Authentication.Services;

public interface IPermissionService {
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> UserHasPermissionAsync(Guid userId, string permissionKey, CancellationToken cancellationToken = default);
}
