namespace Common.Authentication;

public interface IUserContext {
    string? UserId { get; }
    string? EntraIdUserId { get; }
    string? UserEmail { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    string? GetClaim(string claimType);
}
