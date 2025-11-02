using System.Text;
using Microsoft.AspNetCore.Routing;

namespace MovieNight.Gateway.Endpoints;

public static class DebugEndpoints
{
    public static IEndpointRouteBuilder MapDebug(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/debug").WithTags("Debug");

        g.MapGet("/echo", (HttpRequest req) =>
        {
            var query = req.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            var headers = req.Headers.ToDictionary(k => k.Key, v => v.Value.ToString());

            return Results.Ok(new
            {
                method = "GET",
                path = req.Path.ToString(),
                query,
                headers,
                time = DateTimeOffset.UtcNow
            });
        });

        g.MapPost("/echo", async (HttpRequest req) =>
        {
            using var reader = new StreamReader(req.Body, Encoding.UTF8, leaveOpen: false);
            var body = await reader.ReadToEndAsync();
            var headers = req.Headers.ToDictionary(k => k.Key, v => v.Value.ToString());

            return Results.Ok(new
            {
                method = "POST",
                path = req.Path.ToString(),
                headers,
                body
            });
        });

        return routes;
    }
}