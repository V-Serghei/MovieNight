namespace Auth.Infrastructure.Clients;

using System.Net.Http.Json;


public class UsersClient(HttpClient http)
{
    public sealed record VerifyResponse(bool ok, UserDto? user);
    public sealed record UserDto(Guid Id, string Email, string? DisplayName, string? Role, bool IsActive);

    public async Task<VerifyResponse> VerifyAsync(string email, string password, CancellationToken ct)
    {
        var resp = await http.PostAsJsonAsync("/users/verify-credentials", new { Email = email, Password = password }, ct);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<VerifyResponse>(cancellationToken: ct);
        return json!;
    }

    public async Task<UserDto?> GetUser(Guid id, CancellationToken ct)
    {
        var resp = await http.GetAsync($"/users/{id}", ct);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<UserDto>(cancellationToken: ct);
    }
}
