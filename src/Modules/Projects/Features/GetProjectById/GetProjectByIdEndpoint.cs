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

namespace OpenPsa.Modules.Projects.Features.GetProjectById;

public class GetProjectByIdEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/projects/{id:guid}", async (Guid id, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var project = await db.Set<Project>().FirstOrDefaultAsync(p => p.Id == id, ct);
            if (project == null) return Results.NotFound();

            var clientName = (await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(project.ClientId), ct)).Name ?? string.Empty;

            string? managerName = null;
            if (project.ManagerUserId != null && Guid.TryParse(project.ManagerUserId, out var mgId))
                managerName = (await bus.InvokeAsync<GetUserNameResponse>(new GetUserNameQuery(mgId), ct)).Name;

            return Results.Ok(Result.Ok(new ProjectDto {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                ClientId = project.ClientId,
                ClientName = clientName,
                ManagerUserId = project.ManagerUserId,
                ManagerUserName = managerName,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                BudgetHours = project.BudgetHours,
                BudgetAmount = project.BudgetAmount,
                LoggedHours = project.LoggedHours,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            }));
        }).RequirePermission("projects.view").WithTags("Projects");
    }
}
