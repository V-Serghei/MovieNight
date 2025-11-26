using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MovieNight.Gateway.Endpoints;

public static class RatingsProxyEndpoints
{
    public static IEndpointRouteBuilder MapRatingsProxy(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/ratings").WithTags("Ratings");

        // GET /ratings/movies/{movieId}
        g.MapGet("/movies/{movieId:guid}", ProxyPassthrough);
        // GET /ratings/movies?ids=...
        g.MapGet("/movies", ProxyPassthrough);

        // PUT /ratings/movies/{movieId}
        g.MapPut("/movies/{movieId:guid}", ProxyWithBody);
        // DELETE /ratings/movies/{movieId}
        g.MapDelete("/movies/{movieId:guid}", ProxyPassthrough);

        return routes;
    }

    private static async Task ProxyPassthrough(
        HttpContext ctx,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("ratings");
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

    private static async Task ProxyWithBody(
        HttpContext ctx,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("ratings");

        using var msg = CommonProxy.BuildOutgoingMessage(ctx, ctx.Request.Path + ctx.Request.QueryString);
        AddUserHeaders(ctx, msg);

        using var resp = await client.SendAsync(
            msg,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        await CommonProxy.CopyBack(ctx, resp, ct);
    }

    private static void CopyCookie(HttpContext ctx, HttpRequestMessage msg)
    {
        if (ctx.Request.Headers.TryGetValue("Cookie", out var cookie))
            msg.Headers.TryAddWithoutValidation("Cookie", cookie.ToArray());
    }

    private static void CopyAuthOrCookie(HttpContext ctx, HttpRequestMessage msg)
    {
        if (ctx.Request.Headers.TryGetValue("Authorization", out var auth))
        {
            msg.Headers.TryAddWithoutValidation("Authorization", auth.ToArray());
        }
        else if (ctx.Request.Cookies.TryGetValue("access_token", out var accessToken))
        {
            msg.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
        }

        if (ctx.User?.FindFirst("sub")?.Value is { } userId)
        {
            msg.Headers.TryAddWithoutValidation("X-UserId", userId);
        }
    }

    private static async Task ProxyCopyResponse(
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
    private static void AddUserHeaders(HttpContext ctx, HttpRequestMessage msg)
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