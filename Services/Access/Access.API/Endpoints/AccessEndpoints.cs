using Access.API.DTO;
using Access.Core.Entities;
using Access.Core.Enums;
using Access.Core.Repositories;
using Access.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Access.API.Endpoints;

public static class AccessEndpoints
{
    public static IEndpointRouteBuilder MapAccessEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/access").WithTags("Access");

        g.MapPost("/evaluate", async ([FromBody] EvaluateRequestDto req, IAccessEvaluator evaluator, CancellationToken ct) =>
        {
            var (allow, reason, matched) = await evaluator.EvaluateAsync(req.UserId, req.Resource, req.Method, req.Action, req.ContextJson, ct);
            return Results.Ok(new EvaluateResponseDto(allow, reason, matched?.Id.ToString(), matched?.Description));
        });

        g.MapPost("/roles", async ([FromBody] RoleDto dto, IAccessRepository repo, CancellationToken ct) =>
        {
            var role = new Role { Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id, Name = dto.Name };
            await repo.AddRoleAsync(role, ct); await repo.SaveChangesAsync(ct);
            return Results.Created($"/access/roles/{role.Id}", new RoleDto(role.Id, role.Name));
        });

        g.MapPost("/policies", async ([FromBody] PolicyDto dto, IAccessRepository repo, CancellationToken ct) =>
        {
            var policy = new Policy { Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id, Resource = dto.Resource, Method = dto.Method, Action = dto.Action, ConditionJson = dto.ConditionJson, Description = dto.Description, Effect = dto.Effect?.Equals("allow", StringComparison.OrdinalIgnoreCase) == true ? Effect.Allow : Effect.Deny };
            await repo.AddPolicyAsync(policy, ct); await repo.SaveChangesAsync(ct);
            return Results.Created($"/access/policies/{policy.Id}", dto with { Id = policy.Id });
        });

        g.MapPost("/roles/{roleId:guid}/link-policy/{policyId:guid}", async (Guid roleId, Guid policyId, IAccessRepository repo, CancellationToken ct) => { await repo.LinkRolePolicyAsync(roleId, policyId, ct); await repo.SaveChangesAsync(ct); return Results.NoContent(); });
        g.MapPost("/users/{userId:guid}/link-role/{roleId:guid}", async (Guid userId, Guid roleId, IAccessRepository repo, CancellationToken ct) => { await repo.LinkUserRoleAsync(userId, roleId, ct); await repo.SaveChangesAsync(ct); return Results.NoContent(); });
        
        g.MapGet("/users/{userId:guid}/roles", async (Guid userId, IAccessRepository repo, CancellationToken ct) =>
        {
            var roles = await repo.GetRolesForUserAsync(userId, ct); // IEnumerable<Role>
            return Results.Ok(new { roles = roles.Select(r => r.Name).ToArray() });
        });
        return app;
    }
}
