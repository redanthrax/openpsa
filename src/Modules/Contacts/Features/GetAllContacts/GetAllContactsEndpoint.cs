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

namespace OpenPsa.Modules.Contacts.Features.GetAllContacts;

public class GetAllContactsEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/contacts", async (
            OpenPsaDbContext db, IMessageBus bus,
            Guid? clientId,
            int page = 1, int pageSize = 25,
            CancellationToken ct = default) => {

            var query = db.Set<Contact>().AsQueryable();
            if (clientId.HasValue) query = query.Where(c => c.ClientId == clientId.Value);

            var ordered = query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName);
            var totalCount = await ordered.CountAsync(ct);
            var contacts = await ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var clientIds = contacts.Select(c => c.ClientId).Distinct().ToList();
            var clientNames = (await bus.InvokeAsync<GetClientNamesResponse>(new GetClientNamesQuery(clientIds), ct)).Names;

            var dtos = contacts.Select(c => new ContactDto {
                Id = c.Id,
                ClientId = c.ClientId,
                ClientName = clientNames.GetValueOrDefault(c.ClientId, string.Empty),
                FirstName = c.FirstName,
                LastName = c.LastName,
                Title = c.Title,
                Email = c.Email,
                Phone = c.Phone,
                IsPrimary = c.IsPrimary,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();

            return Results.Ok(PagedResult.Ok<ContactDto>(dtos, totalCount, page, pageSize));
        }).RequirePermission("contacts.list").WithTags("Contacts");
    }
}
