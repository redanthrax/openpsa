using Common.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenPsa.Modules.Authentication.Services;

namespace OpenPsa.Modules.Authentication.Features.Auth.Logout;

public record LogoutRequest(string? RefreshToken);

public class LogoutEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/auth/logout", async (
            LogoutRequest? request,
            HttpContext http,
            IRefreshTokenService refreshTokens,
            CancellationToken ct) => {

                if (!string.IsNullOrWhiteSpace(request?.RefreshToken)) {
                    var ip = http.Connection.RemoteIpAddress?.ToString();
                    await refreshTokens.RevokeAsync(request.RefreshToken, ip, ct);
                }
                return Results.Ok();
            }).WithTags("Auth");
    }
}
