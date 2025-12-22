using System.Text;
using System.Text.Json;

namespace MovieNight.Gateway.Endpoints;

public static class UsersPublicEndpoints
{
    public static IEndpointRouteBuilder MapUsersPublic(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/users").WithTags("Users");

        g.MapGet("/me", GetMeBasic).WithOpenApi();

        g.MapGet("/me/profile", GetMyProfileFull).WithOpenApi();

        g.MapPut("/me/profile", UpdateMyProfileFull).WithOpenApi();

        g.MapGet("/{userId:guid}/profile", GetUserProfileFull).WithOpenApi();

        // Requires valid auth cookie or Authorization header
        //g.MapGet("/me", UsersMe).WithOpenApi();
        g.MapGet("/", Users).WithOpenApi();
        return routes;
    }

    // ---------- /users/me ----------

    internal static async Task GetMeBasic(
        HttpContext ctx,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var auth = http.CreateClient("auth");

        using var meReq = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        CopyAuthOrCookie(ctx, meReq);

        using var meResp = await auth.SendAsync(meReq, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!meResp.IsSuccessStatusCode)
        {
            await ProxyCopyResponse(ctx, meResp, ct);
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
        var email = meUser.TryGetProperty("email", out var e) ? e.GetString() : null;
        var displayName = meUser.TryGetProperty("displayName", out var dn) ? dn.GetString() : null;

        // Роли из Access
        string[]? roles = null;
        try
        {
            var access = http.CreateClient("access");
            using var rolesResp = await access.GetAsync($"/access/users/{userId}/roles", ct);
            if (rolesResp.IsSuccessStatusCode)
            {
                using var rdoc = JsonDocument.Parse(await rolesResp.Content.ReadAsStringAsync(ct));
                if (rdoc.RootElement.TryGetProperty("roles", out var arr) &&
                    arr.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<string>();
                    foreach (var el in arr.EnumerateArray())
                    {
                        var s = el.GetString();
                        if (!string.IsNullOrWhiteSpace(s))
                            list.Add(s!);
                    }

                    roles = list.ToArray();
                }
            }
        }
        catch
        {
        }

        ctx.Response.ContentType = "application/json";
        await using var w = new Utf8JsonWriter(ctx.Response.Body);
        w.WriteStartObject();

        w.WritePropertyName("id");
        w.WriteStringValue(userId);

        if (!string.IsNullOrWhiteSpace(email))
        {
            w.WritePropertyName("email");
            w.WriteStringValue(email);
        }

        if (!string.IsNullOrWhiteSpace(displayName))
        {
            w.WritePropertyName("displayName");
            w.WriteStringValue(displayName);
        }

        if (roles is { Length: > 0 })
        {
            w.WritePropertyName("roles");
            w.WriteStartArray();
            foreach (var r in roles)
                w.WriteStringValue(r);
            w.WriteEndArray();
        }

        w.WriteEndObject();
        await w.FlushAsync();
    }

    // ---------- /users/me/profile (GET) ----------

    internal static async Task GetMyProfileFull(
        HttpContext ctx,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var auth = http.CreateClient("auth");

        using var meReq = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        CopyAuthOrCookie(ctx, meReq);

        using var meResp = await auth.SendAsync(meReq, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!meResp.IsSuccessStatusCode)
        {
            await ProxyCopyResponse(ctx, meResp, ct);
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

        await WriteUserProfileResponse(ctx, http, userId, ct);
    }

    // ---------- /users/me/profile (PUT) ----------

    internal static async Task UpdateMyProfileFull(
        HttpContext ctx,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var auth = http.CreateClient("auth");
        using var meReq = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        CopyAuthOrCookie(ctx, meReq);

        using var meResp = await auth.SendAsync(meReq, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!meResp.IsSuccessStatusCode)
        {
            await ProxyCopyResponse(ctx, meResp, ct);
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

        string bodyJson;
        using (var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8))
        {
            bodyJson = await reader.ReadToEndAsync(ct);
        }

        var users = http.CreateClient("users");
        using var updReq = new HttpRequestMessage(HttpMethod.Put, $"/users/{userId}/profile")
        {
            Content = new StringContent(
                string.IsNullOrWhiteSpace(bodyJson) ? "{}" : bodyJson,
                Encoding.UTF8,
                "application/json")
        };

        using var updResp = await users.SendAsync(updReq, HttpCompletionOption.ResponseHeadersRead, ct);

        await ProxyCopyResponse(ctx, updResp, ct);
    }

    // ---------- /users/{userId}/profile (GET) ----------

    internal static async Task GetUserProfileFull(
        Guid userId,
        HttpContext ctx,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        await WriteUserProfileResponse(ctx, http, userId, ct);
    }


    internal static async Task WriteUserProfileResponse(
        HttpContext ctx,
        IHttpClientFactory http,
        Guid userId,
        CancellationToken ct)
    {
        var users = http.CreateClient("users");

        using var userResp = await users.GetAsync($"/users/{userId}", ct);
        if (!userResp.IsSuccessStatusCode)
        {
            await ProxyCopyResponse(ctx, userResp, ct);
            return;
        }

        var userJson = await userResp.Content.ReadAsStringAsync(ct);
        using var udoc = JsonDocument.Parse(userJson);
        var u = udoc.RootElement;

        string? profileJson = null;
        try
        {
            using var profileResp = await users.GetAsync($"/users/{userId}/profile", ct);
            if (profileResp.IsSuccessStatusCode)
                profileJson = await profileResp.Content.ReadAsStringAsync(ct);
        }
        catch
        {
        }

        string[]? roles = null;
        try
        {
            var access = http.CreateClient("access");
            using var rolesResp = await access.GetAsync($"/access/users/{userId}/roles", ct);
            if (rolesResp.IsSuccessStatusCode)
            {
                using var rdoc = JsonDocument.Parse(await rolesResp.Content.ReadAsStringAsync(ct));
                if (rdoc.RootElement.TryGetProperty("roles", out var arr) &&
                    arr.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<string>();
                    foreach (var el in arr.EnumerateArray())
                    {
                        var s = el.GetString();
                        if (!string.IsNullOrWhiteSpace(s))
                            list.Add(s!);
                    }

                    roles = list.ToArray();
                }
            }
        }
        catch
        {
        }

        ctx.Response.ContentType = "application/json";
        await using var w = new Utf8JsonWriter(ctx.Response.Body);

        w.WriteStartObject();

        w.WritePropertyName("id");
        w.WriteStringValue(userId);

        WriteStringIfExists(w, u, "email", "email");
        WriteStringIfExists(w, u, "displayName", "displayName");
        WriteStringIfExists(w, u, "createdAt", "createdAt");
        WriteStringIfExists(w, u, "avatarUrl", "avatarUrl");

        if (profileJson != null)
        {
            using var pdoc = JsonDocument.Parse(profileJson);
            var p = pdoc.RootElement;

            WriteStringIfExists(w, p, "userName", "userName");
            WriteStringIfExists(w, p, "firstName", "firstName");
            WriteStringIfExists(w, p, "lastName", "lastName");
            WriteStringIfExists(w, p, "aboutMe", "aboutMe");
            WriteStringIfExists(w, p, "quote", "quote");
            WriteStringIfExists(w, p, "phoneNumber", "phoneNumber");
            WriteStringIfExists(w, p, "gender", "gender");
            WriteStringIfExists(w, p, "country", "country");
            WriteStringIfExists(w, p, "facebook", "facebook");
            WriteStringIfExists(w, p, "twitter", "twitter");
            WriteStringIfExists(w, p, "instagram", "instagram");
            WriteStringIfExists(w, p, "gitHub", "gitHub");
            WriteStringIfExists(w, p, "avatarMediaId", "avatarMediaId");
            WriteStringIfExists(w, p, "dateOfBirth", "dateOfBirth");

            WriteBoolIfExists(w, p, "personalInfoFriendsOnly", "personalInfoFriendsOnly");
            WriteBoolIfExists(w, p, "showOnlyBasicInfo", "showOnlyBasicInfo");
            WriteBoolIfExists(w, p, "hideBrowsingHistory", "hideBrowsingHistory");
            WriteBoolIfExists(w, p, "hideGrades", "hideGrades");
        }

        if (roles is { Length: > 0 })
        {
            w.WritePropertyName("roles");
            w.WriteStartArray();
            foreach (var r in roles)
                w.WriteStringValue(r);
            w.WriteEndArray();
        }

        w.WriteEndObject();
        await w.FlushAsync();
    }

    internal static async Task Users(HttpContext ctx, IHttpClientFactory http, CancellationToken ct)
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

    internal static void CopyAuthOrCookie(HttpContext ctx, HttpRequestMessage msg)
    {
        if (ctx.Request.Headers.TryGetValue("Authorization", out var auth))
            msg.Headers.TryAddWithoutValidation("Authorization", auth.ToArray());
        else if (ctx.Request.Headers.TryGetValue("Cookie", out var cookie))
            msg.Headers.TryAddWithoutValidation("Cookie", cookie.ToArray());
    }

    internal static async Task ProxyCopyResponse(
        HttpContext ctx,
        HttpResponseMessage resp,
        CancellationToken ct)
    {
        ctx.Response.StatusCode = (int)resp.StatusCode;
        foreach (var h in resp.Headers)
            ctx.Response.Headers[h.Key] = h.Value.ToArray();
        foreach (var h in resp.Content.Headers)
            ctx.Response.Headers[h.Key] = h.Value.ToArray();
        ctx.Response.Headers.Remove("transfer-encoding");
        await resp.Content.CopyToAsync(ctx.Response.Body, ct);
    }

    internal static void WriteStringIfExists(
        Utf8JsonWriter w,
        JsonElement root,
        string sourceName,
        string targetName)
    {
        if (!root.TryGetProperty(sourceName, out var v))
            return;

        if (v.ValueKind is not (JsonValueKind.String or JsonValueKind.Null))
            return;

        w.WritePropertyName(targetName);
        if (v.ValueKind == JsonValueKind.Null)
            w.WriteNullValue();
        else
            w.WriteStringValue(v.GetString());
    }

    internal static void WriteBoolIfExists(
        Utf8JsonWriter w,
        JsonElement root,
        string sourceName,
        string targetName)
    {
        if (!root.TryGetProperty(sourceName, out var v))
            return;

        if (v.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            return;

        w.WritePropertyName(targetName);
        w.WriteBooleanValue(v.GetBoolean());
    }
}