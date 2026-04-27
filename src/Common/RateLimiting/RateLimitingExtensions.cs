using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.RateLimiting;

/// <summary>
/// Built-in AspNetCore rate-limiting policies for the API. Two named policies
/// plus a global limiter:
///   - "auth"  : strict fixed-window per-IP for /api/auth/* (login, refresh, register)
///   - "write" : moderate per-user (or per-IP fallback) limit for mutating endpoints
///   - global  : token-bucket per partition (user-id when authenticated, IP otherwise)
/// All limits are configurable under the "RateLimiting" section.
/// </summary>
public static class RateLimitingExtensions {
    public const string AuthPolicy = "auth";
    public const string WritePolicy = "write";

    public static IServiceCollection AddOpenPsaRateLimiting(this IServiceCollection services, IConfiguration configuration) {
        var section = configuration.GetSection("RateLimiting");
        var auth = section.GetSection("Auth").Get<FixedWindowOptions>() ?? new FixedWindowOptions { PermitLimit = 10, WindowSeconds = 60 };
        var write = section.GetSection("Write").Get<FixedWindowOptions>() ?? new FixedWindowOptions { PermitLimit = 60, WindowSeconds = 60 };
        var global = section.GetSection("Global").Get<TokenBucketOptions>() ?? new TokenBucketOptions { TokenLimit = 200, ReplenishSeconds = 1, TokensPerPeriod = 20 };

        services.AddRateLimiter(options => {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(AuthPolicy, http => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: PartitionByIp(http),
                factory: _ => new FixedWindowRateLimiterOptions {
                    PermitLimit = auth.PermitLimit,
                    Window = TimeSpan.FromSeconds(auth.WindowSeconds),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                }));

            options.AddPolicy(WritePolicy, http => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: PartitionByUserOrIp(http),
                factory: _ => new FixedWindowRateLimiterOptions {
                    PermitLimit = write.PermitLimit,
                    Window = TimeSpan.FromSeconds(write.WindowSeconds),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
                RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: PartitionByUserOrIp(http),
                    factory: _ => new TokenBucketRateLimiterOptions {
                        TokenLimit = global.TokenLimit,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(global.ReplenishSeconds),
                        TokensPerPeriod = global.TokensPerPeriod,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.OnRejected = async (ctx, ct) => {
                ctx.HttpContext.Response.Headers["Retry-After"] =
                    ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                        ? ((int)retry.TotalSeconds).ToString()
                        : "60";
                ctx.HttpContext.Response.ContentType = "application/problem+json";
                await ctx.HttpContext.Response.WriteAsync(
                    """{"type":"https://openpsa.dev/errors/rate-limited","title":"Too many requests","status":429}""", ct);
            };
        });

        return services;
    }

    private static string PartitionByIp(HttpContext http) =>
        http.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static string PartitionByUserOrIp(HttpContext http) {
        var userId = http.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId)) return $"user:{userId}";
        return $"ip:{PartitionByIp(http)}";
    }

    public sealed class FixedWindowOptions {
        public int PermitLimit { get; set; } = 10;
        public int WindowSeconds { get; set; } = 60;
    }

    public sealed class TokenBucketOptions {
        public int TokenLimit { get; set; } = 200;
        public int ReplenishSeconds { get; set; } = 1;
        public int TokensPerPeriod { get; set; } = 20;
    }
}
