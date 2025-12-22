using Microsoft.AspNetCore.Mvc;
using MovieNight.Gateway.Infrastructure;

namespace MovieNight.Gateway.Endpoints;

public static class InternalUsersProxyEndpoints
{
    public static IEndpointRouteBuilder MapInternalUsersProxy(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/_internal/users")
            .WithTags("Internal: Users")
            .AddEndpointFilter(new InternalSecretFilter()); 

        // POST /_internal/users/verify-credentials  ->  Users.Service /users/verify-credentials
        g.MapPost("/verify-credentials", async (HttpContext ctx, IHttpClientFactory http, CancellationToken ct) =>
        {
            var client = http.CreateClient("users");
            using var msg = CommonProxy.BuildOutgoingMessage(ctx, "/users/verify-credentials");
            using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
            await CommonProxy.CopyBack(ctx, resp, ct);
        });

        // GET /_internal/users/{id}  -> Users.Service /users/{id}
        g.MapGet("/{id:guid}", async ([FromRoute] Guid id, HttpContext ctx, IHttpClientFactory http, CancellationToken ct) =>
        {
            var client = http.CreateClient("users");
            using var msg = new HttpRequestMessage(HttpMethod.Get, $"/users/{id}");
            using var resp = await client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
            await CommonProxy.CopyBack(ctx, resp, ct);
        });

        return routes;
    }
}