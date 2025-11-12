using Microsoft.AspNetCore.Mvc;
using MovieNight.Gateway.Infrastructure;

namespace MovieNight.Gateway.Endpoints;

public static class InternalAccessProxyEndpoints
{
    public static IEndpointRouteBuilder MapInternalAccessProxy(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/_internal/access")
            .WithTags("Internal: Access")
            .AddEndpointFilter(new InternalSecretFilter());

        // GET /_internal/access/users/{id}/roles  -> Access /access/users/{id}/roles
        g.MapGet("/users/{id:guid}/roles", async ([FromRoute] Guid id, HttpContext ctx, IHttpClientFactory http, CancellationToken ct) =>
        {
            var client = http.CreateClient("access");
            using var msg = new HttpRequestMessage(HttpMethod.Get, $"/access/users/{id}/roles");
            using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
            await CommonProxy.CopyBack(ctx, resp, ct);
        });

        return routes;
    }
}