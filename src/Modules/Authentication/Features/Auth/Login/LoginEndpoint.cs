using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Authentication.Models;
using OpenPsa.Modules.Authentication.Services;

namespace OpenPsa.Modules.Authentication.Features.Auth.Login;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string UserId, string Email, string Name);

public class LoginEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/auth/login", async (
            LoginRequest request,
            OpenPsaDbContext db,
            IJwtService jwtService,
            IPermissionService permissionService,
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

            return Results.Ok(Result.Ok(new LoginResponse(token, user.Id.ToString(), user.Email, user.Name)));
        }).AllowAnonymous().WithTags("Auth");
    }

}
