using System.Text;
using System.Text.Json;
using MovieNight.Gateway.DTO.Messages;

namespace MovieNight.Gateway.Endpoints;

public static class MessagesProxyEndpoints
{
    public static IEndpointRouteBuilder MapMessagesProxy(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/messages").WithTags("Messages");

        // === GET /messages?senderId=... ===
        g.MapGet("/", ListProxy).WithOpenApi();

        // === GET /messages/{id} ===
        g.MapGet("/{id:guid}", GetByIdProxy).WithOpenApi();

        // === GET /messages/by-receiver/{receiverId} ===
        g.MapGet("/by-receiver/{receiverId}", GetByReceiverIdProxy).WithOpenApi();

        // === POST /messages ===
        g.MapPost("/compose", CreateProxy)
            .WithOpenApi();

        // Проксирование любых других путей, если захочешь
        g.Map("/{**path}", ProxyAny)
            .WithMetadata(new HttpMethodMetadata(
                new[] { "GET", "POST", "PUT", "DELETE", "PATCH" }));

        return routes;
    }
    
    private static async Task ListProxy(HttpContext ctx, IHttpClientFactory http, CancellationToken ct)
    {
        var client = http.CreateClient("messages");

        var senderId = ctx.Request.Query["senderId"].ToString();

        var url = "/messages";
        if (!string.IsNullOrWhiteSpace(senderId))
        {
            url += $"/sent/{senderId}";
        }

        using var msg = new HttpRequestMessage(HttpMethod.Get, url);
        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }
    
    private static async Task GetByIdProxy(HttpContext ctx, IHttpClientFactory http, Guid id, CancellationToken ct)
    {
        var client = http.CreateClient("messages");

        using var msg = new HttpRequestMessage(HttpMethod.Get, $"/messages/{id}");
        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }
    
    private static async Task GetByReceiverIdProxy(HttpContext ctx, IHttpClientFactory http, string receiverId, CancellationToken ct)
    {
        var client = http.CreateClient("messages");

        using var msg = new HttpRequestMessage(HttpMethod.Get, $"/messages/by-receiver/{receiverId}");
        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }
    
    private static async Task CreateProxy(HttpContext ctx, IHttpClientFactory http, CancellationToken ct)
    {
        var client = http.CreateClient("messages");
        
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/messages/compose");
        CopyAuthOrCookie(ctx, msg);
        
        msg.Content = new StreamContent(ctx.Request.Body);
        
        var contentType = ctx.Request.ContentType;
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            msg.Content.Headers.TryAddWithoutValidation("Content-Type", contentType);
        }
        else
        {
            msg.Content.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        }

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    
    private static async Task ProxyAny(HttpContext ctx, IHttpClientFactory http, string path, CancellationToken ct)
    {
        var client = http.CreateClient("messages");

        var uri = $"/messages/{path}{ctx.Request.QueryString}";
        using var msg = new HttpRequestMessage(new HttpMethod(ctx.Request.Method), uri);

        CopyAuthOrCookie(ctx, msg);

        if (ctx.Request.ContentLength > 0)
            msg.Content = new StreamContent(ctx.Request.Body);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }
    
    private static void CopyAuthOrCookie(HttpContext ctx, HttpRequestMessage msg)
    {
        if (ctx.Request.Headers.TryGetValue("Authorization", out var auth))
            msg.Headers.TryAddWithoutValidation("Authorization", (string)auth);

        var cookie = ctx.Request.Headers.Cookie;
        if (!string.IsNullOrEmpty(cookie))
            msg.Headers.TryAddWithoutValidation("Cookie", (string?)cookie!);
    }

    private static async Task ProxyCopyResponse(HttpContext ctx, HttpResponseMessage resp, CancellationToken ct)
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