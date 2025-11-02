// MovieNight.Gateway/Endpoints/AuthProxyEndpoints.cs
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

        return routes;
    }

    private static async Task LoginProxy(
        HttpContext ctx,
        [FromBody] LoginRequest body,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("auth");

        using var msg = new HttpRequestMessage(HttpMethod.Post, "/auth/login")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);

        ctx.Response.StatusCode = (int)resp.StatusCode;

        foreach (var h in resp.Headers)
            ctx.Response.Headers[h.Key] = h.Value.ToArray();
        foreach (var h in resp.Content.Headers)
            ctx.Response.Headers[h.Key] = h.Value.ToArray();
        ctx.Response.Headers.Remove("transfer-encoding");

        await resp.Content.CopyToAsync(ctx.Response.Body, ct);
    }

    private static async Task RegisterProxy(
        HttpContext ctx,
        [FromBody] RegisterRequest body,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var client = http.CreateClient("auth");

        using var msg = new HttpRequestMessage(HttpMethod.Post, "/auth/register")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);

        ctx.Response.StatusCode = (int)resp.StatusCode;
        foreach (var h in resp.Headers)
            ctx.Response.Headers[h.Key] = h.Value.ToArray();
        foreach (var h in resp.Content.Headers)
            ctx.Response.Headers[h.Key] = h.Value.ToArray();
        ctx.Response.Headers.Remove("transfer-encoding");

        await resp.Content.CopyToAsync(ctx.Response.Body, ct);
    }
}
