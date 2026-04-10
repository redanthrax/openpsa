using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Common.Authentication;

public class UserContext : IUserContext {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ClaimsPrincipal? _user;

    public UserContext(IHttpContextAccessor httpContextAccessor) {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _user ??= _httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirst("internal_user_id")?.Value;

    private const string ObjectIdentifierClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    public string? EntraIdUserId => User?.FindFirst(ObjectIdentifierClaimType)?.Value
        ?? User?.FindFirst("oid")?.Value
        ?? User?.FindFirst("sub")?.Value
        ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? UserEmail => User?.FindFirst(ClaimTypes.Email)?.Value
        ?? User?.FindFirst("email")?.Value
        ?? User?.FindFirst("preferred_username")?.Value
        ?? User?.FindFirst("upn")?.Value
        ?? User?.FindFirst(ClaimTypes.Upn)?.Value;

    public string? UserName => User?.FindFirst(ClaimTypes.Name)?.Value
        ?? User?.FindFirst("name")?.Value;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles => User?
        .FindAll(ClaimTypes.Role)
        .Select(c => c.Value) ?? Enumerable.Empty<string>();

    public string? GetClaim(string claimType) => User?.FindFirst(claimType)?.Value;
}
