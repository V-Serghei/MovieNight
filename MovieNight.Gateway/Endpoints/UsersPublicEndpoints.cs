using System.Text.Json;

namespace MovieNight.Gateway.Endpoints;

public static class UsersPublicEndpoints
{
    public static IEndpointRouteBuilder MapUsersPublic(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/users").WithTags("Users");
        // Requires valid auth cookie or Authorization header
        g.MapGet("/me", UsersMe).WithOpenApi();
        g.MapGet("/", Users).WithOpenApi();
        return routes;
    }

    private static async Task UsersMe(HttpContext ctx, IHttpClientFactory http, CancellationToken ct)
    {
        var auth = http.CreateClient("auth");
        using var meMsg = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        CopyAuthOrCookie(ctx, meMsg);
        using var meResp = await auth.SendAsync(meMsg, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!meResp.IsSuccessStatusCode)
        {
            await ProxyCopyResponse(ctx, meResp, ct); // bubble 401/403
            return;
        }

        var meJson = await meResp.Content.ReadAsStringAsync(ct);
        using var meDoc = JsonDocument.Parse(meJson);
        if (!meDoc.RootElement.TryGetProperty("user", out var meUser))
        {
            ctx.Response.StatusCode = 500;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync("{\"error\":\"Invalid /auth/me payload\"}", ct);
            return;
        }

        var userId = meUser.GetProperty("id").GetGuid();

        // Try to enrich from Users service
        string? userJson = null;
        try
        {
            var users = http.CreateClient("users");
            using var userResp = await users.GetAsync($"/users/{userId}", ct);
            if (userResp.IsSuccessStatusCode)
                userJson = await userResp.Content.ReadAsStringAsync(ct);
        }
        catch { /* ignore */ }

        // Try to get roles from Access service
        string[]? roles = null;
        try
        {
            var access = http.CreateClient("access");
            using var rolesResp = await access.GetAsync($"/access/users/{userId}/roles", ct);
            if (rolesResp.IsSuccessStatusCode)
            {
                using var rdoc = JsonDocument.Parse(await rolesResp.Content.ReadAsStringAsync(ct));
                if (rdoc.RootElement.TryGetProperty("roles", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<string>();
                    foreach (var el in arr.EnumerateArray())
                    {
                        var s = el.GetString();
                        if (!string.IsNullOrWhiteSpace(s)) list.Add(s!);
                    }
                    roles = list.ToArray();
                }
            }
        }
        catch { /* ignore */ }

        ctx.Response.ContentType = "application/json";
        await using var w = new Utf8JsonWriter(ctx.Response.Body);
        w.WriteStartObject();
        w.WritePropertyName("id"); w.WriteStringValue(userId);

        w.WritePropertyName("email");
        w.WriteStringValue(meUser.TryGetProperty("email", out var e) ? e.GetString() : null);

        w.WritePropertyName("displayName");
        w.WriteStringValue(meUser.TryGetProperty("displayName", out var dn) ? dn.GetString() : null);

        // Try add avatar/createdAt from Users payload
        if (userJson != null)
        {
            using var udoc = JsonDocument.Parse(userJson);
            var root = udoc.RootElement;
            if (root.TryGetProperty("createdAt", out var ca))
            { w.WritePropertyName("createdAt"); w.WriteStringValue(ca.GetString()); }
            if (root.TryGetProperty("avatarUrl", out var av))
            { w.WritePropertyName("avatarUrl"); w.WriteStringValue(av.GetString()); }
        }

        if (roles is { Length: > 0 })
        {
            w.WritePropertyName("roles");
            w.WriteStartArray();
            foreach (var r in roles) w.WriteStringValue(r);
            w.WriteEndArray();
        }

        w.WriteEndObject();
        await w.FlushAsync();
    }

    private static async Task Users(HttpContext ctx, IHttpClientFactory http, CancellationToken ct)
    {
        // var users = http.CreateClient("users");
        // using var userResp = await users.GetAsync($"/users", ct);
        // if (userResp.IsSuccessStatusCode)
        //     await ProxyCopyResponse(ctx, userResp, ct);
        var client = http.CreateClient("users");

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/users");
        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    private static void CopyAuthOrCookie(HttpContext ctx, HttpRequestMessage msg)
    {
        if (ctx.Request.Headers.TryGetValue("Authorization", out var auth))
            msg.Headers.TryAddWithoutValidation("Authorization", auth.ToArray());
        else if (ctx.Request.Headers.TryGetValue("Cookie", out var cookie))
            msg.Headers.TryAddWithoutValidation("Cookie", cookie.ToArray());
    }

    private static async Task ProxyCopyResponse(HttpContext ctx, HttpResponseMessage resp, CancellationToken ct)
    {
        ctx.Response.StatusCode = (int)resp.StatusCode;
        foreach (var h in resp.Headers) ctx.Response.Headers[h.Key] = h.Value.ToArray();
        foreach (var h in resp.Content.Headers) ctx.Response.Headers[h.Key] = h.Value.ToArray();
        ctx.Response.Headers.Remove("transfer-encoding");
        await resp.Content.CopyToAsync(ctx.Response.Body, ct);
    }
}