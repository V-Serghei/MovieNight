namespace Access.Core.Repositories;

using Access.Core.Entities;


public interface IAccessRepository
{
    Task<IReadOnlyList<Role>> GetUserRolesAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<Policy>> GetPoliciesForRolesAsync(IEnumerable<Guid> roleIds, CancellationToken ct);
    Task<IReadOnlyList<Policy>> GetPoliciesForUserAsync(Guid userId, CancellationToken ct);

    Task<Role> AddRoleAsync(Role role, CancellationToken ct);
    Task<Policy> AddPolicyAsync(Policy policy, CancellationToken ct);
    Task LinkRolePolicyAsync(Guid roleId, Guid policyId, CancellationToken ct);
    Task LinkUserRoleAsync(Guid userId, Guid roleId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task<IReadOnlyList<Role>> GetRolesForUserAsync(Guid userId, CancellationToken ct = default);
}
