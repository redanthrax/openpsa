using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Authentication.Models;

namespace OpenPsa.Modules.Authentication.Features.Users.GetUserById;

public class GetUserByIdEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/users/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var user = await db.Set<User>().FirstOrDefaultAsync(u => u.Id == id, ct);
            if (user == null) return Results.NotFound();

            var roles = await db.Set<Role>()
                .Where(r => user.RoleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync(ct);

            return Results.Ok(Result.Ok(new UserDto {
                Id = user.Id.ToString(),
                Email = user.Email,
                Name = user.Name,
                IsActive = user.IsActive,
                IsSuperAdmin = user.IsSuperAdmin,
                Roles = roles
            }));
        }).RequirePermission("users.view").WithTags("Users");
    }
}
