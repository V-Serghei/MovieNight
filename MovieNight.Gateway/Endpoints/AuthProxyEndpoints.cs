using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MovieNight.Gateway.DTO.User;

namespace MovieNight.Gateway.Endpoints;

public static class AuthProxyEndpoints
{
    public static IEndpointRouteBuilder MapAuthProxy(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/auth").WithTags("Auth");

        g.MapPost("/login", LoginProxy)
         .Accepts<LoginRequest>("application/json")
         .WithOpenApi();

        g.MapPost("/register", RegisterProxy)
         .Accepts<RegisterRequest>("application/json")
         .WithOpenApi();

        g.MapPost("/refresh", RefreshProxy).WithOpenApi();
        g.MapPost("/logout",  LogoutProxy).WithOpenApi();
        g.MapGet ("/me",      MeProxy).WithOpenApi();

        return routes;
    }

    // Tokens.Service
    internal static async Task LoginProxy(HttpContext ctx, [FromBody] LoginRequest body, IHttpClientFactory http, CancellationToken ct)
    {
        var client = http.CreateClient("auth");
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/auth/login")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    // Users.Service
    internal static async Task RegisterProxy(
        HttpContext ctx,
        [FromBody] RegisterRequest body,
        IHttpClientFactory http,
        IConfiguration cfg,
        CancellationToken ct)
    {
        var users = http.CreateClient("users");
        using var regMsg = new HttpRequestMessage(HttpMethod.Post, "/users/register")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        using var regResp = await users.SendAsync(regMsg, HttpCompletionOption.ResponseHeadersRead, ct);
        var regJson = await regResp.Content.ReadAsStringAsync(ct);

        if (!regResp.IsSuccessStatusCode)
        {
            ctx.Response.StatusCode = (int)regResp.StatusCode;
            ctx.Response.ContentType = regResp.Content.Headers.ContentType?.ToString() ?? "application/json";
            await ctx.Response.WriteAsync(regJson, ct);
            return;
        }

        Guid userId;
        try
        {
            using var doc = JsonDocument.Parse(regJson);
            userId = doc.RootElement.GetProperty("id").GetGuid();
        }
        catch
        {
            ctx.Response.StatusCode = 201;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(regJson, ct);
            return;
        }

        var access = http.CreateClient("access");
        var roleId = Guid.Parse(cfg["Access:DefaultUserRoleId"]!);
        using var linkMsg = new HttpRequestMessage(HttpMethod.Post, $"/access/users/{userId}/link-role/{roleId}");
        using var linkResp = await access.SendAsync(linkMsg, ct);

        ctx.Response.StatusCode = 201;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync(regJson, ct);
    }

    // Tokens.Service
    internal static async Task RefreshProxy(HttpContext ctx, IHttpClientFactory http, CancellationToken ct)
    {
        var client = http.CreateClient("auth");
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");
        CopyCookie(ctx, msg);
        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    internal static async Task LogoutProxy(HttpContext ctx, IHttpClientFactory http, CancellationToken ct)
    {
        var client = http.CreateClient("auth");
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/auth/logout");
        CopyCookie(ctx, msg);
        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    internal static async Task MeProxy(HttpContext ctx, IHttpClientFactory http, CancellationToken ct)
    {
        var client = http.CreateClient("auth");
        using var msg = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);

        ctx.Response.StatusCode = (int)resp.StatusCode;
        foreach (var h in resp.Headers)
            ctx.Response.Headers[h.Key] = h.Value.ToArray();
        foreach (var h in resp.Content.Headers)
            ctx.Response.Headers[h.Key] = h.Value.ToArray();

        ctx.Response.Headers.Remove("transfer-encoding");

        ctx.Response.Headers["Cache-Control"] = "no-store, private";
        ctx.Response.Headers["Pragma"] = "no-cache";
        ctx.Response.Headers["Vary"] = "Cookie";

        await resp.Content.CopyToAsync(ctx.Response.Body, ct);
    }

    internal static void CopyCookie(HttpContext ctx, HttpRequestMessage msg)
    {
        if (ctx.Request.Headers.TryGetValue("Cookie", out var cookie))
            msg.Headers.TryAddWithoutValidation("Cookie", cookie.ToArray());
    }

    internal static void CopyAuthOrCookie(HttpContext ctx, HttpRequestMessage msg)
    {
        if (ctx.Request.Headers.TryGetValue("Authorization", out var auth))
            msg.Headers.TryAddWithoutValidation("Authorization", auth.ToArray());
        else
            CopyCookie(ctx, msg);
    }

    internal static async Task ProxyCopyResponse(HttpContext ctx, HttpResponseMessage resp, CancellationToken ct)
    {
        ctx.Response.StatusCode = (int)resp.StatusCode;
        foreach (var h in resp.Headers)
            ctx.Response.Headers[h.Key] = h.Value.ToArray();
        foreach (var h in resp.Content.Headers)
            ctx.Response.Headers[h.Key] = h.Value.ToArray();
        ctx.Response.Headers.Remove("transfer-encoding");
        await resp.Content.CopyToAsync(ctx.Response.Body, ct);
    }
}
