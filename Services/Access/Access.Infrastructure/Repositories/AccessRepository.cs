using Access.Core.Entities;
using Access.Core.Repositories;
using Access.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Access.Infrastructure.Repositories;

public class AccessRepository(AccessDbContext db) : IAccessRepository
{
    public async Task<IReadOnlyList<Role>> GetUserRolesAsync(Guid userId, CancellationToken ct)
    {
        var q =
            from ur in db.UserRoles.AsNoTracking()
            join r in db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ur.UserId == userId
            select r;

        return await q.Distinct().ToListAsync(ct);
    }

    public Task<IReadOnlyList<Role>> GetRolesForUserAsync(Guid userId, CancellationToken ct = default)
        => GetUserRolesAsync(userId, ct);

    public async Task<IReadOnlyList<Policy>> GetPoliciesForRolesAsync(IEnumerable<Guid> roleIds, CancellationToken ct)
    {
        var set = roleIds.ToHashSet();
        var q =
            from rp in db.RolePolicies.AsNoTracking()
            join p in db.Policies.AsNoTracking() on rp.PolicyId equals p.Id
            where set.Contains(rp.RoleId)
            select p;

        return await q.Distinct().ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Policy>> GetPoliciesForUserAsync(Guid userId, CancellationToken ct)
    {
        var q =
            from up in db.UserPolicies.AsNoTracking()
            join p in db.Policies.AsNoTracking() on up.PolicyId equals p.Id
            where up.UserId == userId
            select p;

        return await q.Distinct().ToListAsync(ct);
    }

    public async Task<Role> AddRoleAsync(Role role, CancellationToken ct)
    {
        await db.Roles.AddAsync(role, ct);
        return role;
    }

    public async Task<Policy> AddPolicyAsync(Policy policy, CancellationToken ct)
    {
        await db.Policies.AddAsync(policy, ct);
        return policy;
    }

    public Task LinkRolePolicyAsync(Guid roleId, Guid policyId, CancellationToken ct)
        => db.RolePolicies.AddAsync(new RolePolicy { RoleId = roleId, PolicyId = policyId }, ct).AsTask();

    public Task LinkUserRoleAsync(Guid userId, Guid roleId, CancellationToken ct)
        => db.UserRoles.AddAsync(new UserRole { UserId = userId, RoleId = roleId }, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
