using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieNight.Gateway.Infrastructure;
using Xunit;

namespace Gateway.Tests.Infrastructure;

public class InternalSecretFilterTests
{
    [Fact]
    public async Task AllowsAuthenticatedUser_WithoutHeader()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "123") },
            authenticationType: "test");
        ctx.User = new ClaimsPrincipal(identity);

        var filter = new InternalSecretFilter();

        var invocationContext = new TestEndpointFilterInvocationContext(ctx);

        // Act
        var result = await filter.InvokeAsync(
            invocationContext,
            _ => ValueTask.FromResult<object?>(Results.Ok("ok")));

        // Assert
        Assert.IsType<Ok<string>>(result);
    }

    [Fact]
    public async Task RejectsUnauthenticated_WhenSecretMissingOrInvalid()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Gateway:Secret"] = "expected-secret"
                }!)
                .Build())
            .BuildServiceProvider();

        var ctx = new DefaultHttpContext
        {
            RequestServices = services
        };

        var filter = new InternalSecretFilter();
        var invocationContext = new TestEndpointFilterInvocationContext(ctx);

        // Act
        var result = await filter.InvokeAsync(
            invocationContext,
            _ => throw new Exception("Should not be called"));

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }
}

file sealed class TestEndpointFilterInvocationContext : EndpointFilterInvocationContext
{
    public override HttpContext HttpContext { get; }
    public override object?[] Arguments { get; }

    public TestEndpointFilterInvocationContext(HttpContext httpContext, object?[]? arguments = null)
    {
        HttpContext = httpContext;
        Arguments = arguments ?? Array.Empty<object?>();
    }

    public override T GetArgument<T>(int index)
    {
        return (T)Arguments[index]!;
    }
}
