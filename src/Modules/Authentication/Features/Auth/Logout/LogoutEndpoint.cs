using Common.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace OpenPsa.Modules.Authentication.Features.Auth.Logout;

public class LogoutEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/auth/logout", () => Results.Ok())
            .WithTags("Auth");
    }
}
