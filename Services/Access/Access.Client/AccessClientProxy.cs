using Microsoft.Extensions.Caching.Memory;

namespace Access.Client;

public class AccessClientProxy : IAccessClient
{
    private readonly IAccessClient _inner;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _ttl;

    public AccessClientProxy(IAccessClient inner, IMemoryCache cache, TimeSpan? ttl = null)
    { _inner = inner; _cache = cache; _ttl = ttl ?? TimeSpan.FromSeconds(10); }

    public async Task<bool> IsAllowedAsync(Guid userId, string resource, string method, string? action = null, string? contextJson = null, CancellationToken ct = default)
    {
        var key = $"acl:{userId}:{method}:{resource}:{action}:{contextJson}";
        if (_cache.TryGetValue(key, out bool cached)) return cached;
        var allowed = await _inner.IsAllowedAsync(userId, resource, method, action, contextJson, ct);
        _cache.Set(key, allowed, _ttl);
        return allowed;
    }
}
