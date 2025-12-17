namespace MovieNight.Gateway.Endpoints;

using Microsoft.AspNetCore.Mvc;


public static class MediaProxyEndpoints
{
    public static IEndpointRouteBuilder MapMediaProxy(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/media").WithTags("Media");

        g.MapPost("/", UploadProxy).WithOpenApi();
        g.MapGet("/{id:guid}", DownloadProxy).WithOpenApi();
        g.MapGet("/{id:guid}/info", InfoProxy).WithOpenApi();
        g.MapDelete("/{id:guid}", DeleteProxy).WithOpenApi();

        return routes;
    }

    // POST /media  -> Media.API /media
    internal static async Task UploadProxy(
        HttpContext ctx,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("media");

        using var msg = CommonProxy.BuildOutgoingMessage(ctx, "/media");

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await CommonProxy.CopyBack(ctx, resp, ct);
    }


    // GET /media/{id}
    internal static async Task DownloadProxy(
        HttpContext ctx,
        [FromRoute] Guid id,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("media");
        using var msg = new HttpRequestMessage(HttpMethod.Get, $"/media/{id}");
        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    // GET /media/{id}/info
    internal static async Task InfoProxy(
        HttpContext ctx,
        [FromRoute] Guid id,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("media");
        using var msg = new HttpRequestMessage(HttpMethod.Get, $"/media/{id}/info");
        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    // DELETE /media/{id}
    internal static async Task DeleteProxy(
        HttpContext ctx,
        [FromRoute] Guid id,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("media");
        using var msg = new HttpRequestMessage(HttpMethod.Delete, $"/media/{id}");
        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
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
}
