using Microsoft.AspNetCore.Mvc;

namespace MovieNight.Gateway.Endpoints;

public static class MoviesProxyEndpoints
{
    public static IEndpointRouteBuilder MapMoviesProxy(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/movies").WithTags("Movies");

        g.Map("/{**path}", ProxyAny).WithMetadata(new HttpMethodMetadata(new[] { "GET", "POST", "PUT", "DELETE", "PATCH" }));

        return routes;
    }

    private static async Task ProxyAny(
        [FromRoute] string? path,
        HttpContext ctx,
        IHttpClientFactory http,
        CancellationToken ct)
    {
        var target = "/movies" + (string.IsNullOrEmpty(path) ? "" : "/" + path);
        var client = http.CreateClient("movies");

        using var msg = MovieNight.Gateway.Endpoints.CommonProxy.BuildOutgoingMessage(ctx, target);
        using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);

        await CommonProxy.CopyBack(ctx, resp, ct);
    }
}