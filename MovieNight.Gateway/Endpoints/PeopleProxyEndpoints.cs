using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace MovieNight.Gateway.Endpoints;

public static class PeopleProxyEndpoints
{
    public static IEndpointRouteBuilder MapPeopleProxy(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/people").WithTags("People");

        g.MapPost("/", CreatePersonProxy).WithOpenApi();
        g.MapGet("/{id:guid}", GetPersonByIdProxy).WithOpenApi();
        g.MapGet("/search", SearchPeopleProxy).WithOpenApi();

        g.MapPost("/credits", CreateCreditProxy).WithOpenApi();
        g.MapGet("/movies/{movieId:guid}/credits", GetCreditsForMovieProxy).WithOpenApi();

        return routes;
    }

    internal static async Task CreatePersonProxy(
        HttpContext ctx,
        [FromBody] object body,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("people");
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/people")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        CopyAuthOrCookie(ctx, msg);
        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    internal static async Task GetPersonByIdProxy(
        HttpContext ctx,
        [FromRoute] Guid id,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("people");
        using var msg = new HttpRequestMessage(HttpMethod.Get, $"/people/{id}");
        CopyAuthOrCookie(ctx, msg);
        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    internal static async Task SearchPeopleProxy(
        HttpContext ctx,
        [FromQuery] string name,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("people");
        var url = $"/people/search?name={Uri.EscapeDataString(name)}";
        using var msg = new HttpRequestMessage(HttpMethod.Get, url);
        CopyAuthOrCookie(ctx, msg);
        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    internal static async Task CreateCreditProxy(
        HttpContext ctx,
        [FromBody] object body,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("people");
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/people/credits")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        CopyAuthOrCookie(ctx, msg);
        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
        await ProxyCopyResponse(ctx, resp, ct);
    }

    internal static async Task GetCreditsForMovieProxy(
        HttpContext ctx,
        [FromRoute] Guid movieId,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("people");
        using var msg = new HttpRequestMessage(HttpMethod.Get, $"/people/movies/{movieId}/credits");
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

    internal static async Task ProxyCopyResponse(HttpContext ctx, HttpResponseMessage resp, CancellationToken ct)
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