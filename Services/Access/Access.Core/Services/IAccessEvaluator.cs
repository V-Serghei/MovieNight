using Access.Core.Entities;

namespace Access.Core.Services;

public interface  IAccessEvaluator
{
    Task<(bool allow, string reason, Policy? matched)> EvaluateAsync(Guid userId, string resource, string method, string? action, string? contextJson, CancellationToken ct);
}
