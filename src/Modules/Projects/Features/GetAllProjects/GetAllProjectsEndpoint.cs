using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Projects;
using Contracts.Results;
using IntegrationEvents.Authentication;
using IntegrationEvents.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Projects.Models;
using Wolverine;

namespace OpenPsa.Modules.Projects.Features.GetAllProjects;

public class GetAllProjectsEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/projects", async (
            OpenPsaDbContext db, IMessageBus bus,
            Guid? clientId,
            int page = 1, int pageSize = 25,
            CancellationToken ct = default) => {

                var query = db.Set<Project>().AsQueryable();
                if (clientId.HasValue) query = query.Where(p => p.ClientId == clientId.Value);

                var ordered = query.OrderBy(p => p.Name);
                var totalCount = await ordered.CountAsync(ct);
                var projects = await ordered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

                var clientIds = projects.Select(p => p.ClientId).Distinct().ToList();
                var clientNames = (await bus.InvokeAsync<GetClientNamesResponse>(new GetClientNamesQuery(clientIds), ct)).Names;

                var managerIds = projects.Where(p => p.ManagerUserId != null)
                    .Select(p => Guid.Parse(p.ManagerUserId!)).Distinct().ToList();
                var managerNames = (await bus.InvokeAsync<GetUserNamesResponse>(new GetUserNamesQuery(managerIds), ct)).Names;

                var dtos = projects.Select(p => new ProjectDto {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Status = p.Status,
                    ClientId = p.ClientId,
                    ClientName = clientNames.GetValueOrDefault(p.ClientId, string.Empty),
                    ManagerUserId = p.ManagerUserId,
                    ManagerUserName = p.ManagerUserId != null && Guid.TryParse(p.ManagerUserId, out var mgId)
                        ? managerNames.GetValueOrDefault(mgId)
                        : null,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    BudgetHours = p.BudgetHours,
                    BudgetAmount = p.BudgetAmount,
                    LoggedHours = p.LoggedHours,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                return Results.Ok(PagedResult.Ok<ProjectDto>(dtos, totalCount, page, pageSize));
            }).RequirePermission("projects.list").WithTags("Projects");
    }
}
