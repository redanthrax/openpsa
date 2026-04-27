using Common.Database;
using Common.Modules;
using Common.RateLimiting;
using Common.Validation;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Authentication.Models;
using OpenPsa.Modules.Authentication.Services;

namespace OpenPsa.Modules.Authentication.Features.Auth.Login;

public record LoginRequest(string Email, string Password);
public record LoginResponse(
    string Token,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    string UserId,
    string Email,
    string Name);

public class LoginEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/auth/login", async (
            LoginRequest request,
            HttpContext http,
            OpenPsaDbContext db,
            IJwtService jwtService,
            IPermissionService permissionService,
            IRefreshTokenService refreshTokens,
            CancellationToken ct) => {

                var user = await db.Set<User>()
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive, ct);

                if (user == null || string.IsNullOrEmpty(user.LocalPasswordHash))
                    return Results.Json(Result.Fail<LoginResponse>("Invalid email or password"), statusCode: 401);

                if (!PasswordHasher.Verify(request.Password, user.LocalPasswordHash))
                    return Results.Json(Result.Fail<LoginResponse>("Invalid email or password"), statusCode: 401);

                user.LastLoginAt = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);

                var permissions = await permissionService.GetUserPermissionsAsync(user.Id, ct);
                var token = jwtService.GenerateToken(user.Id, user.Email, user.Name, user.IsSuperAdmin, permissions);
                var ip = http.Connection.RemoteIpAddress?.ToString();
                var (refresh, refreshExp) = await refreshTokens.IssueAsync(user.Id, ip, ct);

                return Results.Ok(Result.Ok(new LoginResponse(
                    token, refresh, refreshExp,
                    user.Id.ToString(), user.Email, user.Name)));
            }).AllowAnonymous()
              .WithTags("Auth")
              .WithValidation()
              .RequireRateLimiting(RateLimitingExtensions.AuthPolicy);
    }
}
