namespace Access.Client;

public interface IAccessClient
{
    Task<bool> IsAllowedAsync(Guid userId, string resource, string method, string? action = null, string? contextJson = null, CancellationToken ct = default);
}
