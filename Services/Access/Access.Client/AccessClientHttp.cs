using System.Net.Http.Json;

namespace Access.Client;

public class AccessClientHttp(HttpClient http) : IAccessClient
{
    private sealed record EvaluateRequest(Guid UserId, string Resource, string Method, string? Action, string? ContextJson);
    private sealed record EvaluateResponse(bool Allow, string Reason, string? MatchedPolicyId, string? MatchedDescription);

    public async Task<bool> IsAllowedAsync(Guid userId, string resource, string method, string? action = null, string? contextJson = null, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync("/access/evaluate", new EvaluateRequest(userId, resource, method, action, contextJson), ct);
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync<EvaluateResponse>(cancellationToken: ct);
        return dto?.Allow ?? false;
    }
}
