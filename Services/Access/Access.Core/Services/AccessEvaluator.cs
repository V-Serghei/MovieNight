using System.Text.Json;
using System.Text.RegularExpressions;
using Access.Core.Entities;
using Access.Core.Enums;
using Access.Core.Repositories;

namespace Access.Core.Services;

public class AccessEvaluator(IAccessRepository repo) : IAccessEvaluator
{
    public async Task<(bool allow, string reason, Policy? matched)> EvaluateAsync(Guid userId, string resource, string method, string? action, string? contextJson, CancellationToken ct)
    {
        var roles = await repo.GetUserRolesAsync(userId, ct);
        var roleIds = roles.Select(r => r.Id).ToArray();
        var rolePolicies = await repo.GetPoliciesForRolesAsync(roleIds, ct);
        var userPolicies = await repo.GetPoliciesForUserAsync(userId, ct);
        var all = rolePolicies.Concat(userPolicies).ToList();

        Policy? match = all
            .Where(p => IsMatch(resource, p.Resource) && (p.Method == null || p.Method.Equals(method, StringComparison.OrdinalIgnoreCase)) && (p.Action == null || p.Action.Equals(action, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(p => SpecificityScore(p.Resource))
            .ThenByDescending(p => (p.Method != null ? 1 : 0) + (p.Action != null ? 1 : 0))
            .FirstOrDefault();

        if (match is null)
            return (false, "No matching policy", null);

        if (!CheckCondition(match.ConditionJson, contextJson))
            return (false, "Condition not satisfied", match);

        var allow = match.Effect == Effect.Allow;
        return (allow, allow ? "Allowed by policy" : "Denied by policy", match);
    }

    private static bool IsMatch(string resource, string pattern)
    {
        var regex = "^" + Regex.Escape(pattern)
            .Replace(@"\\*\\*", ".*")
            .Replace(@"\\*", "[^/]+") + "$";
        return Regex.IsMatch(resource, regex, RegexOptions.IgnoreCase);
    }

    private static int SpecificityScore(string pattern)
        => pattern.Count(c => c != '*' && c != '/');

    private static bool CheckCondition(string? policyCond, string? ctxJson)
    {
        if (string.IsNullOrWhiteSpace(policyCond)) return true;
        if (string.IsNullOrWhiteSpace(ctxJson)) return false;
        try
        {
            using var pDoc = JsonDocument.Parse(policyCond);
            using var cDoc = JsonDocument.Parse(ctxJson);
            if (pDoc.RootElement.TryGetProperty("ownerOnly", out var ownerOnly) && ownerOnly.GetBoolean())
            {
                return cDoc.RootElement.TryGetProperty("owner", out var owner) && owner.GetBoolean();
            }
            return true;
        }
        catch { return false; }
    }
}
