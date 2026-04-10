using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Contacts;
using Contracts.Results;
using IntegrationEvents.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Contacts.Models;
using Wolverine;

namespace OpenPsa.Modules.Contacts.Features.UpdateContact;

public class UpdateContactEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/contacts/{id:guid}", async (Guid id, UpdateContactRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var contact = await db.Set<Contact>().FirstOrDefaultAsync(c => c.Id == id, ct);
            if (contact == null) return Results.NotFound();

            contact.FirstName = request.FirstName;
            contact.LastName = request.LastName;
            contact.Title = request.Title;
            contact.Email = request.Email;
            contact.Phone = request.Phone;
            contact.IsPrimary = request.IsPrimary;

            await db.SaveChangesAsync(ct);

            var clientName = (await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(contact.ClientId), ct)).Name ?? string.Empty;

            return Results.Ok(Result.Ok(new ContactDto {
                Id = contact.Id,
                ClientId = contact.ClientId,
                ClientName = clientName,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Title = contact.Title,
                Email = contact.Email,
                Phone = contact.Phone,
                IsPrimary = contact.IsPrimary,
                CreatedAt = contact.CreatedAt,
                UpdatedAt = contact.UpdatedAt
            }));
        }).RequirePermission("contacts.update").WithTags("Contacts");
    }
}
