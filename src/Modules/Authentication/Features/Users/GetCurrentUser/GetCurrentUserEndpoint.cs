using Common.Authentication;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Authentication.Models;
using OpenPsa.Modules.Authentication.Services;

namespace OpenPsa.Modules.Authentication.Features.Users.GetCurrentUser;

public class GetCurrentUserEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/users/me", async (
            IUserContext userContext,
            OpenPsaDbContext db,
            IPermissionService permissionService,
            CancellationToken ct) => {

            if (!Guid.TryParse(userContext.UserId, out var userId))
                return Results.Unauthorized();

            var user = await db.Set<User>().FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user == null) return Results.Unauthorized();

            var roles = await db.Set<Role>()
                .Where(r => user.RoleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync(ct);

            var permissions = await permissionService.GetUserPermissionsAsync(userId, ct);

            return Results.Ok(Result.Ok(new CurrentUserDto {
                Id = user.Id.ToString(),
                Email = user.Email,
                Name = user.Name,
                IsSuperAdmin = user.IsSuperAdmin,
                Roles = roles,
                Permissions = permissions.ToList()
            }));
        }).WithTags("Users");
    }
}
