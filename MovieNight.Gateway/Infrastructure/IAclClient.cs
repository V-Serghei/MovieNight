namespace MovieNight.Gateway.Infrastructure;

public interface IAclClient
{
    Task<bool> IsAllowedAsync(Guid userId, string resource, string method, CancellationToken ct = default);
}