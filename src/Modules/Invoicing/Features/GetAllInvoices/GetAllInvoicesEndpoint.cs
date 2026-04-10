using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Invoicing;
using Contracts.Results;
using IntegrationEvents.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Invoicing.Models;
using Wolverine;

namespace OpenPsa.Modules.Invoicing.Features.GetAllInvoices;

public class GetAllInvoicesEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/invoices", async (OpenPsaDbContext db, IMessageBus bus, Guid? clientId, InvoiceStatus? status, CancellationToken ct) => {
            var query = db.Set<Invoice>().AsQueryable();
            if (clientId.HasValue) query = query.Where(i => i.ClientId == clientId.Value);
            if (status.HasValue) query = query.Where(i => i.Status == status.Value);

            var invoices = await query.Include(i => i.LineItems).OrderByDescending(i => i.InvoiceDate).ToListAsync(ct);

            var clientIds = invoices.Select(i => i.ClientId).Distinct().ToList();
            var clientNames = (await bus.InvokeAsync<GetClientNamesResponse>(new GetClientNamesQuery(clientIds), ct)).Names;

            var dtos = invoices.Select(i => new InvoiceSummaryDto {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                ClientName = clientNames.GetValueOrDefault(i.ClientId, string.Empty),
                Status = i.Status,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                Total = i.Total,
                AmountDue = i.AmountDue
            });

            return Results.Ok(Result.Ok(dtos));
        }).RequirePermission("invoices.list").WithTags("Invoicing");
    }
}
