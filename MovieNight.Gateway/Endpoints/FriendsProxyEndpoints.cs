using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace MovieNight.Gateway.Endpoints;

public static class FriendsProxyEndpoints
{
    public static IEndpointRouteBuilder MapFriendsProxy(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/friends").WithTags("Friends");

        g.MapGet("/{userId}", GetByUserIdProxy).WithOpenApi();

        g.MapPost("/",
            CreateFriendProxy)
            .Accepts<object>("application/json")
            .WithOpenApi();
        
        g.Map("/{**path}", ProxyAny)
            .WithMetadata(new HttpMethodMetadata(new[] { "GET", "POST", "PUT", "DELETE", "PATCH" }));

        return routes;
    }

    // GET /friends/{userId}
    private static async Task GetByUserIdProxy(
        HttpContext ctx,
        [FromRoute] string userId,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("friends");

        using var msg = new HttpRequestMessage(HttpMethod.Get, $"/friends/{userId}");
        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    // POST /friends
    private static async Task CreateFriendProxy(
        HttpContext ctx,
        [FromBody] object body,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("friends");

        using var msg = new HttpRequestMessage(HttpMethod.Post, "/friends")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    // === UNIVERSAL passthrough /friends/**
    private static async Task ProxyAny(
        [FromRoute] string? path,
        HttpContext ctx,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("friends");

        var target = "/friends" + (string.IsNullOrEmpty(path) ? "" : "/" + path);

        using var msg = new HttpRequestMessage(new HttpMethod(ctx.Request.Method),
            target + ctx.Request.QueryString);

        // Тело запроса (если есть)
        if (ctx.Request.ContentLength > 0 || ctx.Request.Headers.ContainsKey("Content-Type"))
        {
            using var sr = new StreamReader(ctx.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await sr.ReadToEndAsync(ct);
            ctx.Request.Body.Position = 0;

            msg.Content = new StringContent(body, Encoding.UTF8,
                ctx.Request.ContentType ?? "application/json");
        }

        // Копируем заголовки
        foreach (var (k, v) in ctx.Request.Headers)
        {
            if (!msg.Headers.TryAddWithoutValidation(k, (IEnumerable<string>)v))
                msg.Content?.Headers.TryAddWithoutValidation(k, (IEnumerable<string>)v);
        }

        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }


    // ===== helpers =====

    private static void CopyCookie(HttpContext ctx, HttpRequestMessage msg)
    {
        if (ctx.Request.Headers.TryGetValue("Cookie", out var cookie))
            msg.Headers.TryAddWithoutValidation("Cookie", cookie.ToArray());
    }

    private static void CopyAuthOrCookie(HttpContext ctx, HttpRequestMessage msg)
    {
        if (ctx.Request.Headers.TryGetValue("Authorization", out var auth))
            msg.Headers.TryAddWithoutValidation("Authorization", auth.ToArray());
        else
            CopyCookie(ctx, msg);
    }

    private static async Task ProxyCopyResponse(
        HttpContext ctx,
        HttpResponseMessage resp,
        CancellationToken ct)
    {
        ctx.Response.StatusCode = (int)resp.StatusCode;

        foreach (var h in resp.Headers)
            ctx.Response.Headers[h.Key] = h.Value.ToArray();

        if (resp.Content != null)
        {
            foreach (var h in resp.Content.Headers)
                ctx.Response.Headers[h.Key] = h.Value.ToArray();
        }

        ctx.Response.Headers.Remove("transfer-encoding");

        if (resp.Content != null)
            await resp.Content.CopyToAsync(ctx.Response.Body, ct);
    }
}