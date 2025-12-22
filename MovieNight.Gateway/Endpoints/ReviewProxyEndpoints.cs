using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MovieNight.Gateway.Endpoints;

public static class ReviewProxyEndpoints
{
     public static IEndpointRouteBuilder MapReviewsProxy(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/review").WithTags("Review");

        // === GET /review/{filmId} ===
        g.MapGet("/{filmId}", GetByFilmIdProxy).WithOpenApi();

        // === POST /review ===
        g.MapPost("/", CreateReviewProxy)
            .WithOpenApi();
        
        g.Map("/{**path}", ProxyAny)
            .WithMetadata(new HttpMethodMetadata(
                new[] { "GET", "POST", "PUT", "DELETE", "PATCH" }));

        return routes;
    }
    
    internal static async Task GetByFilmIdProxy(
        HttpContext ctx,
        IHttpClientFactory http,
        string filmId,
        CancellationToken ct)
    {
        var client = http.CreateClient("review");

        using var msg = new HttpRequestMessage(HttpMethod.Get, $"/review/{filmId}");
        CopyAuthOrCookie(ctx, msg);
        AddUserHeaders(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }
    
    internal static async Task CreateReviewProxy(
        HttpContext ctx,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("review");

        using var msg = new HttpRequestMessage(HttpMethod.Post, "/review");
        CopyAuthOrCookie(ctx, msg);
        AddUserHeaders(ctx, msg);
        
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
    
    internal static async Task ProxyAny(
        HttpContext ctx,
        IHttpClientFactory http,
        string path,
        CancellationToken ct)
    {
        var client = http.CreateClient("review");

        var uri = $"/review/{path}{ctx.Request.QueryString}";
        using var msg = new HttpRequestMessage(new HttpMethod(ctx.Request.Method), uri);

        CopyAuthOrCookie(ctx, msg);
        AddUserHeaders(ctx, msg);

        if (ctx.Request.ContentLength > 0)
            msg.Content = new StreamContent(ctx.Request.Body);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    internal static void CopyAuthOrCookie(HttpContext ctx, HttpRequestMessage msg)
    {
        if (ctx.Request.Headers.TryGetValue("Authorization", out var auth))
            msg.Headers.TryAddWithoutValidation("Authorization", (string)auth);

        var cookie = ctx.Request.Headers.Cookie;
        if (!string.IsNullOrEmpty(cookie))
            msg.Headers.TryAddWithoutValidation("Cookie", (string?)cookie!);
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
    
    internal static void AddUserHeaders(HttpContext ctx, HttpRequestMessage msg)
    {
        var user = ctx.User;
        var idStr = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!string.IsNullOrWhiteSpace(idStr))
            msg.Headers.TryAddWithoutValidation("X-UserId", idStr);

        var role = user.FindFirstValue(ClaimTypes.Role);
        if (!string.IsNullOrWhiteSpace(role))
            msg.Headers.TryAddWithoutValidation("X-UserRole", role);
        var userName = user.FindFirstValue(ClaimTypes.Name)
                       ?? user.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrWhiteSpace(userName))
            msg.Headers.TryAddWithoutValidation("X-UserName", userName);
    }
}