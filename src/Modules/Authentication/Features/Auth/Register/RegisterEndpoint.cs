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

namespace OpenPsa.Modules.Authentication.Features.Auth.Register;

public record RegisterRequest(string Email, string Password, string Name);

public class RegisterEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/auth/register", async (
            RegisterRequest request,
            OpenPsaDbContext db,
            IJwtService jwtService,
            IPermissionService permissionService,
            CancellationToken ct) => {

            var exists = await db.Set<User>().AnyAsync(u => u.Email == request.Email, ct);
            if (exists)
                return Results.Json(Result.Fail<UserDto>("A user with this email already exists"), statusCode: 409);

            var user = new User {
                Email = request.Email,
                Name = request.Name,
                LocalPasswordHash = PasswordHasher.Hash(request.Password),
                IsActive = true
            };

            db.Set<User>().Add(user);
            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(new UserDto {
                Id = user.Id.ToString(),
                Email = user.Email,
                Name = user.Name,
                IsActive = user.IsActive
            }));
        }).AllowAnonymous().WithTags("Auth");
    }

}
