using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Clients;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenPsa.Modules.Clients.Models;

namespace OpenPsa.Modules.Clients.Features.CreateClient;

public class CreateClientEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/clients", async (CreateClientRequest request, OpenPsaDbContext db, CancellationToken ct) => {
            var client = new Client {
                Name = request.Name,
                Website = request.Website,
                Phone = request.Phone,
                Email = request.Email,
                Notes = request.Notes,
                Status = request.Status
            };

            db.Set<Client>().Add(client);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/clients/{client.Id}", Result.Ok(new ClientDto {
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
        }).RequirePermission("clients.create").WithTags("Clients");
    }
}
