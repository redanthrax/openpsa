using System.Security.Cryptography;
using Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenPsa.Modules.Authentication.Models;

namespace OpenPsa.Modules.Authentication.Services;

public class RefreshTokenService : IRefreshTokenService {
    private readonly OpenPsaDbContext _db;
    private readonly TimeSpan _lifetime;

    public RefreshTokenService(OpenPsaDbContext db, IConfiguration cfg) {
        _db = db;
        var days = int.TryParse(cfg["Jwt:RefreshTokenDays"], out var d) ? d : 14;
        _lifetime = TimeSpan.FromDays(days);
    }

    public async Task<(string RawToken, DateTime ExpiresAt)> IssueAsync(Guid userId, string? ip, CancellationToken ct) {
        var (raw, hash) = GenerateToken();
        var entity = new RefreshToken {
            UserId = userId,
            TokenHash = hash,
            ExpiresAt = DateTime.UtcNow.Add(_lifetime),
            CreatedByIp = ip,
        };
        _db.Set<RefreshToken>().Add(entity);
        await _db.SaveChangesAsync(ct);
        return (raw, entity.ExpiresAt);
    }

    public async Task<RotationResult?> RotateAsync(string rawToken, string? ip, CancellationToken ct) {
        if (string.IsNullOrWhiteSpace(rawToken)) return null;
        var hash = Hash(rawToken);
        var existing = await _db.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (existing is null) return null;

        // Reuse of a rotated/revoked token — kill the whole family for this user.
        if (!existing.IsActive) {
            await RevokeFamilyAsync(existing.UserId, ip, ct);
            return null;
        }

        var (newRaw, newHash) = GenerateToken();
        var replacement = new RefreshToken {
            UserId = existing.UserId,
            TokenHash = newHash,
            ExpiresAt = DateTime.UtcNow.Add(_lifetime),
            CreatedByIp = ip,
        };
        _db.Set<RefreshToken>().Add(replacement);

        existing.RevokedAt = DateTime.UtcNow;
        existing.RevokedByIp = ip;
        existing.ReplacedByTokenId = replacement.Id;

        await _db.SaveChangesAsync(ct);
        return new RotationResult(existing.UserId, newRaw, replacement.ExpiresAt);
    }

    public async Task RevokeAsync(string rawToken, string? ip, CancellationToken ct) {
        if (string.IsNullOrWhiteSpace(rawToken)) return;
        var hash = Hash(rawToken);
        var existing = await _db.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (existing is null || !existing.IsActive) return;
        existing.RevokedAt = DateTime.UtcNow;
        existing.RevokedByIp = ip;
        await _db.SaveChangesAsync(ct);
    }

    private async Task RevokeFamilyAsync(Guid userId, string? ip, CancellationToken ct) {
        var active = await _db.Set<RefreshToken>()
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(ct);
        var now = DateTime.UtcNow;
        foreach (var t in active) {
            t.RevokedAt = now;
            t.RevokedByIp = ip;
        }
        await _db.SaveChangesAsync(ct);
    }

    private static (string Raw, string Hash) GenerateToken() {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var raw = Base64UrlEncode(bytes);
        return (raw, Hash(raw));
    }

    private static string Hash(string raw) {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }

    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
