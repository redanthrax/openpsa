using Contracts.Users;

namespace OpenPsa.Web.Features.Authentication.Services;

public partial class UserPermissionService : IUserPermissionService {
    private readonly IApiClient _api;
    private readonly ILogger<UserPermissionService> _logger;
    private HashSet<string> _permissions = [];

    public CurrentUserDto? CurrentUser { get; private set; }
    public bool IsInitialized { get; private set; }
    public bool IsSuperAdmin => CurrentUser?.IsSuperAdmin ?? false;
    public event Action? OnPermissionsChanged;

    public UserPermissionService(IApiClient api, ILogger<UserPermissionService> logger) {
        _api = api;
        _logger = logger;
    }

    public async Task InitializeAsync() {
        if (IsInitialized) return;
        try {
            var result = await _api.GetAsync<CurrentUserDto>("/api/users/me");
            if (result.Success && result.Data is not null) {
                CurrentUser = result.Data;
                _permissions = [..result.Data.Permissions];
                IsInitialized = true;
                LogLoaded(_logger, _permissions.Count, CurrentUser.Email);
                OnPermissionsChanged?.Invoke();
            }
        } catch (Exception ex) {
            LogFailed(_logger, ex);
        }
    }

    public void Reset() {
        CurrentUser = null;
        _permissions = [];
        IsInitialized = false;
    }

    public bool HasPermission(string p) => IsSuperAdmin || _permissions.Contains(p);
    public bool HasAnyPermission(params string[] ps) => IsSuperAdmin || ps.Any(_permissions.Contains);
    public bool HasAllPermissions(params string[] ps) => IsSuperAdmin || ps.All(_permissions.Contains);

    [LoggerMessage(Level = LogLevel.Information, Message = "Loaded {Count} permissions for {Email}")]
    private static partial void LogLoaded(ILogger l, int count, string email);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to load user permissions")]
    private static partial void LogFailed(ILogger l, Exception ex);
}
