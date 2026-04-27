using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.Sla;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Features.GetAllSlaPolicies;

public class GetAllSlaPoliciesEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/sla-policies", async (
            OpenPsaDbContext db,
            int page = 1, int pageSize = 25,
            CancellationToken ct = default) => {

                var query = db.Set<SlaPolicy>()
                    .Include(p => p.Targets)
                    .OrderByDescending(p => p.CreatedAt);

                var totalCount = await db.Set<SlaPolicy>().CountAsync(ct);
                var policies = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

                var calendarIds = policies
                    .Where(p => p.BusinessHoursCalendarId.HasValue)
                    .Select(p => p.BusinessHoursCalendarId!.Value)
                    .Distinct()
                    .ToList();

                var calendarNames = calendarIds.Count > 0
                    ? await db.Set<BusinessHoursCalendar>()
                        .Where(c => calendarIds.Contains(c.Id))
                        .ToDictionaryAsync(c => c.Id, c => c.Name, ct)
                    : new Dictionary<Guid, string>();

                var dtos = policies.Select(p => new SlaPolicySummaryDto {
                    Id = p.Id,
                    Name = p.Name,
                    IsDefault = p.IsDefault,
                    BusinessHoursCalendarName = p.BusinessHoursCalendarId.HasValue
                        ? calendarNames.GetValueOrDefault(p.BusinessHoursCalendarId.Value)
                        : null,
                    TargetCount = p.Targets.Count
                }).ToList();

                return Results.Ok(PagedResult.Ok<SlaPolicySummaryDto>(dtos, totalCount, page, pageSize));
            }).RequirePermission("sla-policies.list").WithTags("SLA");
    }
}
