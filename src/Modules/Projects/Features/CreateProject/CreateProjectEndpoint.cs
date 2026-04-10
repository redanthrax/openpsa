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

namespace OpenPsa.Modules.Projects.Features.CreateProject;

public class CreateProjectEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/projects", async (CreateProjectRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var clientResponse = await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(request.ClientId), ct);
            if (!clientResponse.Found)
                return Results.Json(Result.Fail<ProjectDto>("Client not found"), statusCode: 404);

            string? managerName = null;
            if (request.ManagerUserId != null && Guid.TryParse(request.ManagerUserId, out var mgId))
                managerName = (await bus.InvokeAsync<GetUserNameResponse>(new GetUserNameQuery(mgId), ct)).Name;

            var project = new Project {
                Name = request.Name,
                Description = request.Description,
                ClientId = request.ClientId,
                ManagerUserId = request.ManagerUserId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                BudgetHours = request.BudgetHours,
                BudgetAmount = request.BudgetAmount
            };

            db.Set<Project>().Add(project);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/projects/{project.Id}", Result.Ok(new ProjectDto {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                ClientId = project.ClientId,
                ClientName = clientResponse.Name ?? string.Empty,
                ManagerUserId = project.ManagerUserId,
                ManagerUserName = managerName,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                BudgetHours = project.BudgetHours,
                BudgetAmount = project.BudgetAmount,
                LoggedHours = 0,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            }));
        }).RequirePermission("projects.create").WithTags("Projects");
    }
}
