using System.Text.Json;

namespace MovieNight.Gateway.Infrastructure;

public sealed class AclClientHttp(HttpClient http) : IAclClient
{
    public async Task<bool> IsAllowedAsync(Guid userId, string resource, string method, CancellationToken ct = default)
    {
        var url = $"/access/authorize?userId={userId}&resource={Uri.EscapeDataString(resource)}&method={method}";
        using var resp = await http.GetAsync(url, ct);
        if (!resp.IsSuccessStatusCode) return false;

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        return doc.RootElement.TryGetProperty("allowed", out var v) && v.GetBoolean();
    }
}