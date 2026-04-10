using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Clients;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Clients.Models;

namespace OpenPsa.Modules.Clients.Features.UpdateClient;

public class UpdateClientEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/clients/{id:guid}", async (Guid id, UpdateClientRequest request, OpenPsaDbContext db, CancellationToken ct) => {
            var client = await db.Set<Client>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (client == null) return Results.NotFound();

            client.Name = request.Name;
            client.Website = request.Website;
            client.Phone = request.Phone;
            client.Email = request.Email;
            client.Notes = request.Notes;
            client.Status = request.Status;

            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(new ClientDto {
                Id = client.Id,
                Name = client.Name,
                Website = client.Website,
                Phone = client.Phone,
                Email = client.Email,
                Notes = client.Notes,
                Status = client.Status,
                CreatedAt = client.CreatedAt,
                UpdatedAt = client.UpdatedAt
            }));
        }).RequirePermission("clients.update").WithTags("Clients");
    }
}
