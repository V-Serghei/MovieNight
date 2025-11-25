namespace MovieNight.Gateway.Endpoints;

public static class ReviewProxyEndpoints
{
     public static IEndpointRouteBuilder MapReviewsProxy(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/reviews").WithTags("Reviews");

        // === GET /reviews/{filmId} ===
        g.MapGet("/{filmId}", GetByFilmIdProxy).WithOpenApi();

        // === POST /reviews ===
        g.MapPost("/", CreateReviewProxy)
            .WithOpenApi();

        // Проксирование любых других путей, если вдруг понадобится
        g.Map("/{**path}", ProxyAny)
            .WithMetadata(new HttpMethodMetadata(
                new[] { "GET", "POST", "PUT", "DELETE", "PATCH" }));

        return routes;
    }
    
    private static async Task GetByFilmIdProxy(
        HttpContext ctx,
        IHttpClientFactory http,
        string filmId,
        CancellationToken ct)
    {
        var client = http.CreateClient("reviews");

        using var msg = new HttpRequestMessage(HttpMethod.Get, $"/reviews/{filmId}");
        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }
    
    private static async Task CreateReviewProxy(
        HttpContext ctx,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("reviews");

        using var msg = new HttpRequestMessage(HttpMethod.Post, "/reviews");
        CopyAuthOrCookie(ctx, msg);

        // Проксируем тело как есть
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
    
    private static async Task ProxyAny(
        HttpContext ctx,
        IHttpClientFactory http,
        string path,
        CancellationToken ct)
    {
        var client = http.CreateClient("reviews");

        var uri = $"/reviews/{path}{ctx.Request.QueryString}";
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

    private static async Task ProxyCopyResponse(
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
}