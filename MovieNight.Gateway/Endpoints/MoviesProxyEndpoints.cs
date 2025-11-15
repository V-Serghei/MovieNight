using System.Text;
using Microsoft.AspNetCore.Mvc;
using MovieNight.Gateway.DTO.Movie;
using System.Text.Json;

namespace MovieNight.Gateway.Endpoints;

public static class MoviesProxyEndpoints
{
    public static IEndpointRouteBuilder MapMoviesProxy(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/movies").WithTags("Movies");
        
        g.MapGet ("/",           ListProxy).WithOpenApi();
        g.MapGet ("/{id:guid}",  GetByIdProxy).WithOpenApi();
        g.MapGet ("/search",     SearchProxy).WithOpenApi();

        g.MapPost("/",           CreateProxy)
            .Accepts<CreateMovieRequest>("application/json")
            .WithOpenApi();

        g.MapPost("/seed",       SeedProxy)
            .Accepts<SeedMoviesRequest>("application/json")
            .WithOpenApi();

        g.MapGet("/films",       FilmsAliasProxy).WithOpenApi();
        g.Map("/{**path}", ProxyAny).WithMetadata(new HttpMethodMetadata(new[] { "GET", "POST", "PUT", "DELETE", "PATCH" }));

        return routes;
    }
    
    // GET /movies
    private static async Task ListProxy(HttpContext ctx, IHttpClientFactory http, CancellationToken ct)
    {
        var client = http.CreateClient("movies");
        var qs = ctx.Request.QueryString.HasValue ? ctx.Request.QueryString.Value : "";
        using var msg = new HttpRequestMessage(HttpMethod.Get, "/movies" + qs);
        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    // GET /movies/{id}
    private static async Task GetByIdProxy(HttpContext ctx, [FromRoute] Guid id, IHttpClientFactory http, CancellationToken ct)
    {
        var client = http.CreateClient("movies");
        using var msg = new HttpRequestMessage(HttpMethod.Get, $"/movies/{id}");
        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    // GET /movies/search?title=&year=&director=
    private static async Task SearchProxy(
        HttpContext ctx,
        [FromQuery] string title,
        [FromQuery] int year,
        [FromQuery] string director,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("movies");
        var url = $"/movies/search?title={Uri.EscapeDataString(title)}&year={year}&director={Uri.EscapeDataString(director)}";
        using var msg = new HttpRequestMessage(HttpMethod.Get, url);
        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    // POST /movies
    private static async Task CreateProxy(HttpContext ctx, [FromBody] CreateMovieRequest body, IHttpClientFactory http, CancellationToken ct)
    {
        var client = http.CreateClient("movies");
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/movies")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    // POST /movies/seed
    private static async Task SeedProxy(HttpContext ctx, [FromBody] SeedMoviesRequest body, IHttpClientFactory http, CancellationToken ct)
    {
        var client = http.CreateClient("movies");
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/movies/seed")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }
    
    private static async Task FilmsAliasProxy(HttpContext ctx, IHttpClientFactory http, CancellationToken ct)
    {
        var client = http.CreateClient("movies");
        var url = "/movies/by-category/Film";
        using var msg = new HttpRequestMessage(HttpMethod.Get, url);
        //CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    private static async Task ProxyAny(
        [FromRoute] string? path,
        HttpContext ctx,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var target = "/movies" + (string.IsNullOrEmpty(path) ? "" : "/" + path);
        var client = http.CreateClient("movies");

        using var msg = new HttpRequestMessage(new HttpMethod(ctx.Request.Method), target + ctx.Request.QueryString);
        // перенести тело, заголовки и т.п.
        if (ctx.Request.ContentLength > 0 || ctx.Request.Headers.ContainsKey("Content-Type"))
        {
            using var sr = new StreamReader(ctx.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await sr.ReadToEndAsync(ct);
            ctx.Request.Body.Position = 0;
            msg.Content = new StringContent(body, Encoding.UTF8, ctx.Request.ContentType ?? "application/json");
        }
        foreach (var (k, v) in ctx.Request.Headers)
        {
            if (!msg.Headers.TryAddWithoutValidation(k, (IEnumerable<string>)v))
            {
                msg.Content?.Headers.TryAddWithoutValidation(k, (IEnumerable<string>)v);
            }
        }
        CopyAuthOrCookie(ctx, msg);

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    // ===== helpers (как в AuthProxyEndpoints) =====
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

    private static async Task ProxyCopyResponse(HttpContext ctx, HttpResponseMessage resp, CancellationToken ct)
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