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

namespace OpenPsa.Modules.Projects.Features.UpdateProject;

public class UpdateProjectEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/projects/{id:guid}", async (Guid id, UpdateProjectRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var project = await db.Set<Project>().FirstOrDefaultAsync(p => p.Id == id, ct);
            if (project == null) return Results.NotFound();

            project.Name = request.Name;
            project.Description = request.Description;
            project.Status = request.Status;
            project.ManagerUserId = request.ManagerUserId;
            project.StartDate = request.StartDate;
            project.EndDate = request.EndDate;
            project.BudgetHours = request.BudgetHours;
            project.BudgetAmount = request.BudgetAmount;

            await db.SaveChangesAsync(ct);

            string? managerName = null;
            if (project.ManagerUserId != null && Guid.TryParse(project.ManagerUserId, out var mgId))
                managerName = (await bus.InvokeAsync<GetUserNameResponse>(new GetUserNameQuery(mgId), ct)).Name;

            var clientName = (await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(project.ClientId), ct)).Name ?? string.Empty;

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
        }).RequirePermission("projects.update").WithTags("Projects");
    }
}
