namespace OpenPsa.Modules.Authentication.Services;

public interface IRefreshTokenService {
    /// <summary>Generate, persist (as hash), and return the raw refresh token for a user.</summary>
    Task<(string RawToken, DateTime ExpiresAt)> IssueAsync(Guid userId, string? ip, CancellationToken ct);

    /// <summary>
    /// Validate a presented raw token and rotate it. Returns the new raw token + the user
    /// it belongs to, or null if the token is unknown / expired / revoked.
    /// On detected reuse of an already-rotated token, the entire token family for that
    /// user is revoked and null is returned.
    /// </summary>
    Task<RotationResult?> RotateAsync(string rawToken, string? ip, CancellationToken ct);

    /// <summary>Revoke a single token (used on logout). No-op if unknown.</summary>
    Task RevokeAsync(string rawToken, string? ip, CancellationToken ct);
}

public record RotationResult(Guid UserId, string NewRawToken, DateTime NewExpiresAt);
