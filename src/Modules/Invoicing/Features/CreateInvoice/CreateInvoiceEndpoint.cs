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

namespace OpenPsa.Modules.Invoicing.Features.CreateInvoice;

public class CreateInvoiceEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/invoices", async (CreateInvoiceRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var clientResponse = await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(request.ClientId), ct);
            if (!clientResponse.Found)
                return Results.Json(Result.Fail<InvoiceDto>("Client not found"), statusCode: 404);

            var number = await GenerateInvoiceNumberAsync(db, ct);

            var invoice = new Invoice {
                InvoiceNumber = number,
                ClientId = request.ClientId,
                InvoiceDate = request.InvoiceDate,
                DueDate = request.DueDate,
                Notes = request.Notes,
                LineItems = request.LineItems.Select(l => new InvoiceLineItem {
                    Description = l.Description,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice
                }).ToList()
            };

            db.Set<Invoice>().Add(invoice);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/invoices/{invoice.Id}", Result.Ok(new InvoiceDto {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                ClientId = invoice.ClientId,
                ClientName = clientResponse.Name ?? string.Empty,
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
        }).RequirePermission("invoices.create").WithTags("Invoicing");
    }

    private static async Task<string> GenerateInvoiceNumberAsync(OpenPsaDbContext db, CancellationToken ct) {
        var count = await db.Set<Invoice>().CountAsync(ct);
        return $"INV-{DateTime.UtcNow.Year}-{(count + 1):D4}";
    }
}
