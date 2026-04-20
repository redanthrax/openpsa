using Common.Authorization;
using Common.Database;
using Common.Modules;
using IntegrationEvents.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenPsa.Modules.Invoicing.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Wolverine;

namespace OpenPsa.Modules.Invoicing.Features.ExportPdf;

public class ExportInvoicePdfEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/invoices/{id:guid}/pdf", async (Guid id, OpenPsaDbContext db, IMessageBus bus, IConfiguration config, CancellationToken ct) => {
            var invoice = await db.Set<Invoice>().Include(i => i.LineItems).FirstOrDefaultAsync(i => i.Id == id, ct);
            if (invoice is null) return Results.NotFound();

            var clientName = (await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(invoice.ClientId), ct)).Name ?? "Unknown";

            var companyName = config["Company:Name"] ?? "OpenPSA";
            var companyEmail = config["Company:Email"];
            var companyPhone = config["Company:Phone"];
            var currency = config["Company:Currency"] ?? "USD";

            var document = new InvoicePdfDocument(invoice, clientName, companyName, companyEmail, companyPhone, currency);
            var pdf = document.GeneratePdf();

            return Results.File(pdf, "application/pdf", $"Invoice-{invoice.InvoiceNumber}.pdf");
        }).RequirePermission("invoices.view").WithTags("Invoicing");
    }
}

internal class InvoicePdfDocument : IDocument {
    private readonly Invoice _invoice;
    private readonly string _clientName;
    private readonly string _companyName;
    private readonly string? _companyEmail;
    private readonly string? _companyPhone;
    private readonly string _currency;

    public InvoicePdfDocument(Invoice invoice, string clientName, string companyName, string? companyEmail, string? companyPhone, string currency) {
        _invoice = invoice;
        _clientName = clientName;
        _companyName = companyName;
        _companyEmail = companyEmail;
        _companyPhone = companyPhone;
        _currency = currency;
    }

    public DocumentMetadata GetMetadata() => new() { Title = $"Invoice {_invoice.InvoiceNumber}" };

    public void Compose(IDocumentContainer container) {
        container.Page(page => {
            page.Size(PageSizes.A4);
            page.MarginHorizontal(40);
            page.MarginVertical(30);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container) {
        container.PaddingBottom(20).Row(row => {
            row.RelativeItem().Column(col => {
                col.Item().Text(_companyName).FontSize(20).Bold().FontColor(Colors.Blue.Darken3);
                if (!string.IsNullOrEmpty(_companyEmail))
                    col.Item().Text(_companyEmail).FontSize(9).FontColor(Colors.Grey.Darken1);
                if (!string.IsNullOrEmpty(_companyPhone))
                    col.Item().Text(_companyPhone).FontSize(9).FontColor(Colors.Grey.Darken1);
            });

            row.RelativeItem().AlignRight().Column(col => {
                col.Item().Text("INVOICE").FontSize(24).Bold().FontColor(Colors.Blue.Darken3);
                col.Item().Text($"#{_invoice.InvoiceNumber}").FontSize(12);
                col.Item().PaddingTop(5).Text($"Date: {_invoice.InvoiceDate:MMM d, yyyy}").FontSize(9);
                col.Item().Text($"Due: {_invoice.DueDate:MMM d, yyyy}").FontSize(9);
                col.Item().PaddingTop(3).Text($"Status: {_invoice.Status}").FontSize(9).Bold();
            });
        });
    }

    private void ComposeContent(IContainer container) {
        container.Column(col => {
            col.Item().PaddingBottom(15).Background(Colors.Grey.Lighten4).Padding(10).Column(info => {
                info.Item().Text("Bill To").Bold().FontSize(11);
                info.Item().Text(_clientName).FontSize(10);
            });

            col.Item().Table(table => {
                table.ColumnsDefinition(columns => {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.5f);
                });

                table.Header(header => {
                    var headerStyle = TextStyle.Default.Bold().FontSize(9).FontColor(Colors.White);

                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Description").Style(headerStyle);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignRight().Text("Qty").Style(headerStyle);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignRight().Text("Unit Price").Style(headerStyle);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignRight().Text("Amount").Style(headerStyle);
                });

                var alt = false;
                foreach (var item in _invoice.LineItems) {
                    var bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                    alt = !alt;

                    table.Cell().Background(bg).Padding(5).Text(item.Description);
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(item.Quantity.ToString("N2"));
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(FormatCurrency(item.UnitPrice));
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(FormatCurrency(item.Amount));
                }
            });

            col.Item().PaddingTop(15).AlignRight().Width(200).Column(totals => {
                TotalRow(totals, "Subtotal", _invoice.Subtotal);
                if (_invoice.TaxRate > 0)
                    TotalRow(totals, $"Tax ({_invoice.TaxRate}%)", _invoice.TaxAmount);
                totals.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                TotalRow(totals, "Total", _invoice.Total, bold: true);
                if (_invoice.AmountPaid > 0)
                    TotalRow(totals, "Paid", -_invoice.AmountPaid);
                if (_invoice.AmountDue != _invoice.Total)
                    TotalRow(totals, "Amount Due", _invoice.AmountDue, bold: true);
            });

            if (!string.IsNullOrEmpty(_invoice.Notes)) {
                col.Item().PaddingTop(25).Column(notes => {
                    notes.Item().Text("Notes").Bold().FontSize(11);
                    notes.Item().PaddingTop(3).Text(_invoice.Notes).FontSize(9).FontColor(Colors.Grey.Darken2);
                });
            }
        });
    }

    private void TotalRow(ColumnDescriptor col, string label, decimal amount, bool bold = false) {
        col.Item().PaddingVertical(2).Row(row => {
            var style = bold ? TextStyle.Default.Bold() : TextStyle.Default;
            row.RelativeItem().Text(label).Style(style);
            row.RelativeItem().AlignRight().Text(FormatCurrency(amount)).Style(style);
        });
    }

    private void ComposeFooter(IContainer container) {
        container.AlignCenter().Text(t => {
            t.Span("Generated by ").FontSize(8).FontColor(Colors.Grey.Darken1);
            t.Span(_companyName).FontSize(8).FontColor(Colors.Grey.Darken1).Bold();
        });
    }

    private string FormatCurrency(decimal amount) => $"{_currency} {amount:N2}";
}
