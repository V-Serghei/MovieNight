using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MovieNight.Gateway.Endpoints;

public static class BookmarksProxyEndpoints
{
    public static IEndpointRouteBuilder MapBookmarksProxy(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/bookmarks").WithTags("Bookmarks");

        // GET
        g.MapGet("/", ProxyPassthrough);
        g.MapGet("/temp", ProxyPassthrough);
        g.MapGet("/watched", ProxyPassthrough);

        // POST
        g.MapPost("/", ProxyWithBody);
        g.MapPost("/temp", ProxyWithBody);
        g.MapPost("/watched", ProxyWithBody);

        // DELETE
        g.MapDelete("/{movieId:guid}", ProxyPassthrough);
        g.MapDelete("/temp/{movieId:guid}", ProxyPassthrough);
        g.MapDelete("/watched/{movieId:guid}", ProxyPassthrough);
        g.MapDelete("/movie/{movieId:guid}/all", ProxyPassthrough);

        return routes;
    }

    internal static async Task ProxyPassthrough(
        HttpContext ctx,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("bookmarks");

        var method = new HttpMethod(ctx.Request.Method);
        var targetPath = ctx.Request.Path + ctx.Request.QueryString;

        using var msg = new HttpRequestMessage(method, targetPath);
        CopyAuthOrCookie(ctx, msg);
        AddUserHeaders(ctx, msg); 

        using var resp = await client.SendAsync(
            msg,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        await ProxyCopyResponse(ctx, resp, ct);
    }

    internal static async Task ProxyWithBody(
        HttpContext ctx,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("bookmarks");

        var targetPath = ctx.Request.Path + ctx.Request.QueryString;

        using var msg = CommonProxy.BuildOutgoingMessage(ctx, targetPath);
        AddUserHeaders(ctx, msg);

        using var resp = await client.SendAsync(
            msg,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        await CommonProxy.CopyBack(ctx, resp, ct);
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

    internal static async Task ProxyCopyResponse(
        HttpContext ctx,
        HttpResponseMessage resp,
        CancellationToken ct)
    {
        ctx.Response.StatusCode = (int)resp.StatusCode;

        foreach (var h in resp.Headers)
            ctx.Response.Headers[h.Key] = h.Value.ToArray();

        if (resp.Content is not null)
        {
            foreach (var h in resp.Content.Headers)
                ctx.Response.Headers[h.Key] = h.Value.ToArray();

            ctx.Response.Headers.Remove("transfer-encoding");
            await resp.Content.CopyToAsync(ctx.Response.Body, ct);
        }
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
    }
}