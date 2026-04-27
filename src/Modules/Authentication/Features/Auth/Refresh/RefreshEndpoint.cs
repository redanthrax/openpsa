using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Authentication.Models;
using OpenPsa.Modules.Authentication.Services;

namespace OpenPsa.Modules.Authentication.Features.Auth.Refresh;

public record RefreshRequest(string RefreshToken);
public record RefreshResponse(string Token, string RefreshToken, DateTime RefreshTokenExpiresAt);

public class RefreshEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/auth/refresh", async (
            RefreshRequest request,
            HttpContext http,
            OpenPsaDbContext db,
            IJwtService jwtService,
            IPermissionService permissionService,
            IRefreshTokenService refreshTokens,
            CancellationToken ct) => {

                var ip = http.Connection.RemoteIpAddress?.ToString();
                var rotated = await refreshTokens.RotateAsync(request.RefreshToken, ip, ct);
                if (rotated is null)
                    return Results.Json(Result.Fail<RefreshResponse>("Invalid or expired refresh token"), statusCode: 401);

                var user = await db.Set<User>()
                    .FirstOrDefaultAsync(u => u.Id == rotated.UserId && u.IsActive, ct);
                if (user is null)
                    return Results.Json(Result.Fail<RefreshResponse>("User no longer active"), statusCode: 401);

                var permissions = await permissionService.GetUserPermissionsAsync(user.Id, ct);
                var access = jwtService.GenerateToken(user.Id, user.Email, user.Name, user.IsSuperAdmin, permissions);

                return Results.Ok(Result.Ok(new RefreshResponse(
                    access, rotated.NewRawToken, rotated.NewExpiresAt)));
            }).AllowAnonymous().WithTags("Auth");
    }
}
