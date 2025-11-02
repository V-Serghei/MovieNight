using Microsoft.AspNetCore.Routing;

namespace MovieNight.Gateway.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealth(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/healthz", () => Results.Ok(new { status = "ok", time = DateTimeOffset.UtcNow }))
            .WithName("Healthz")
            .WithTags("Health");

        routes.MapGet("/readyz", () => Results.Ok(new { ready = true }))
            .WithName("Readyz")
            .WithTags("Health");

        return routes;
    }
}