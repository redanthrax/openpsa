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

namespace OpenPsa.Modules.Invoicing.Features.GetInvoiceById;

public class GetInvoiceByIdEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/invoices/{id:guid}", async (Guid id, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var invoice = await db.Set<Invoice>().Include(i => i.LineItems).FirstOrDefaultAsync(i => i.Id == id, ct);
            if (invoice == null) return Results.NotFound();

            var clientName = (await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(invoice.ClientId), ct)).Name ?? string.Empty;

            return Results.Ok(Result.Ok(new InvoiceDto {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                ClientId = invoice.ClientId,
                ClientName = clientName,
                Status = invoice.Status,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                Subtotal = invoice.Subtotal,
                TaxAmount = invoice.TaxAmount,
                Total = invoice.Total,
                AmountPaid = invoice.AmountPaid,
                AmountDue = invoice.AmountDue,
                Notes = invoice.Notes,
                CreatedAt = invoice.CreatedAt,
                UpdatedAt = invoice.UpdatedAt,
                LineItems = invoice.LineItems.Select(l => new InvoiceLineItemDto {
                    Id = l.Id,
                    Description = l.Description,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    Amount = l.Amount
                }).ToList()
            }));
        }).RequirePermission("invoices.view").WithTags("Invoicing");
    }
}
