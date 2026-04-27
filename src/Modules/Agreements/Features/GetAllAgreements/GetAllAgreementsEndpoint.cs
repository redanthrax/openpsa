using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Agreements;
using Contracts.Results;
using IntegrationEvents.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Agreements.Models;
using Wolverine;

namespace OpenPsa.Modules.Agreements.Features.GetAllAgreements;

public class GetAllAgreementsEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/agreements", async (
            OpenPsaDbContext db, IMessageBus bus,
            Guid? clientId, AgreementStatus? status,
            int page = 1, int pageSize = 25,
            CancellationToken ct = default) => {

                var query = db.Set<Agreement>().AsQueryable();
                if (clientId.HasValue) query = query.Where(a => a.ClientId == clientId.Value);
                if (status.HasValue) query = query.Where(a => a.Status == status.Value);

                var ordered = query.OrderByDescending(a => a.CreatedAt);
                var totalCount = await ordered.CountAsync(ct);
                var agreements = await ordered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

                var clientIds = agreements.Select(a => a.ClientId).Distinct().ToList();
                var clientNames = (await bus.InvokeAsync<GetClientNamesResponse>(new GetClientNamesQuery(clientIds), ct)).Names;

                var dtos = agreements.Select(a => new AgreementSummaryDto {
                    Id = a.Id,
                    Name = a.Name,
                    Type = a.Type,
                    Status = a.Status,
                    ClientName = clientNames.GetValueOrDefault(a.ClientId, string.Empty),
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    MonthlyAmount = a.MonthlyAmount,
                    BlockHoursRemaining = a.BlockHoursTotal.HasValue ? a.BlockHoursTotal.Value - (a.BlockHoursUsed ?? 0) : null
                }).ToList();

                return Results.Ok(PagedResult.Ok<AgreementSummaryDto>(dtos, totalCount, page, pageSize));
            }).RequirePermission("agreements.list").WithTags("Agreements");
    }
}
