using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Authentication.Models;
using OpenPsa.Modules.Authentication.Services;

namespace OpenPsa.Modules.Authentication.Features.Auth.ExternalCallback;

public record ExternalCallbackRequest(string Provider, string SubjectId, string Email, string Name);

public class ExternalCallbackEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/auth/external-callback", async (
            ExternalCallbackRequest request,
            OpenPsaDbContext db,
            IJwtService jwtService,
            IPermissionService permissionService,
            CancellationToken ct) => {

                var user = await db.Set<User>()
                    .FirstOrDefaultAsync(u => u.ExternalProvider == request.Provider && u.ExternalSubjectId == request.SubjectId, ct)
                    ?? await db.Set<User>().FirstOrDefaultAsync(u => u.Email == request.Email, ct);

                if (user == null) {
                    user = new User {
                        Email = request.Email,
                        Name = request.Name,
                        ExternalProvider = request.Provider,
                        ExternalSubjectId = request.SubjectId
                    };
                    db.Set<User>().Add(user);
                } else {
                    user.ExternalProvider ??= request.Provider;
                    user.ExternalSubjectId ??= request.SubjectId;
                }

                if (!user.IsActive)
                    return Results.Json(Result.Fail<string>("Account is disabled"), statusCode: 403);

                user.LastLoginAt = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);

                var permissions = await permissionService.GetUserPermissionsAsync(user.Id, ct);
                var token = jwtService.GenerateToken(user.Id, user.Email, user.Name, user.IsSuperAdmin, permissions);

                return Results.Ok(Result.Ok(token));
            }).AllowAnonymous().WithTags("Auth");
    }
}
