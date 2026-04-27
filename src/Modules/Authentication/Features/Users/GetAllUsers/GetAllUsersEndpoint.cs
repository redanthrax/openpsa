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

namespace OpenPsa.Modules.Authentication.Features.Users.GetAllUsers;

public class GetAllUsersEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/users", async (
            OpenPsaDbContext db,
            int page = 1, int pageSize = 25,
            CancellationToken ct = default) => {

                var ordered = db.Set<User>().OrderBy(u => u.Name);
                var totalCount = await ordered.CountAsync(ct);
                var users = await ordered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

                var roles = await db.Set<Role>().ToListAsync(ct);

                var dtos = users.Select(u => new UserDto {
                    Id = u.Id.ToString(),
                    Email = u.Email,
                    Name = u.Name,
                    IsActive = u.IsActive,
                    IsSuperAdmin = u.IsSuperAdmin,
                    Roles = roles.Where(r => u.RoleIds.Contains(r.Id)).Select(r => r.Name).ToList()
                }).ToList();

                return Results.Ok(PagedResult.Ok<UserDto>(dtos, totalCount, page, pageSize));
            }).RequirePermission("users.list").WithTags("Users");
    }
}
