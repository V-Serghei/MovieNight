using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure;

public sealed class InternalSecretFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var cfg = ctx.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var expected = cfg["Gateway:Secret"] ?? "dev-internal-secret";
        if (!ctx.HttpContext.Request.Headers.TryGetValue("X-Internal-Secret", out var actual) || actual != expected)
            return Results.Unauthorized();
        return await next(ctx);
    }
}