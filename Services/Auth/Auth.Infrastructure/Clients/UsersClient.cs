namespace Auth.Infrastructure.Clients;

using System.Net.Http.Json;


public class UsersClient(HttpClient http)
{
    public sealed record VerifyResponse(bool ok, UserDto? user);
    public sealed record UserDto(Guid Id, string Email, string? DisplayName, bool IsActive);

    public async Task<VerifyResponse> VerifyAsync(string email, string password, CancellationToken ct)
    {
        var resp = await http.PostAsJsonAsync("/_internal/users/verify-credentials", new { Email = email, Password = password }, ct);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<VerifyResponse>(cancellationToken: ct))!;
    }

    public async Task<UserDto?> GetUser(Guid id, CancellationToken ct)
    {
        var resp = await http.GetAsync($"/_internal/users/{id}", ct);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<UserDto>(cancellationToken: ct);
    }
}
