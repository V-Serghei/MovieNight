using System.Net.Http.Json;

namespace Auth.Infrastructure.Clients;

public class AccessClient(HttpClient http)
{
    public sealed record RolesResponse(string[] roles);

    public async Task<string?> GetPrimaryRoleAsync(Guid userId, CancellationToken ct)
    {
        var resp = await http.GetAsync($"/_internal/access/users/{userId}/roles", ct);
        if (!resp.IsSuccessStatusCode) return null;

        var dto = await resp.Content.ReadFromJsonAsync<RolesResponse>(cancellationToken: ct);
        var roles = dto?.roles ?? Array.Empty<string>();
        if (roles.Contains("admin")) return "admin";
        if (roles.Contains("user"))  return "user";
        return roles.FirstOrDefault();
    }
}