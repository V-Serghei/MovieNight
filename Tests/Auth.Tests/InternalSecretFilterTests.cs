using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Auth.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Auth.Tests;

public class InternalSecretFilterTests
{
    private static TestEndpointFilterInvocationContext CreateContext(string? secret = null)
    {
        var http = new DefaultHttpContext();

        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Gateway:Secret"] = "super-secret"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(cfg);

        http.RequestServices = services.BuildServiceProvider();

        if (secret != null)
            http.Request.Headers["X-Internal-Secret"] = secret;

        return new TestEndpointFilterInvocationContext(http);
    }

    private static EndpointFilterDelegate Next(object? resultToReturn)
        => _ => ValueTask.FromResult(resultToReturn);

    [Fact]
    public async Task Should_ReturnUnauthorized_When_HeaderMissing()
    {
        var ctx = CreateContext(null);
        var filter = new InternalSecretFilter();

        var result = await filter.InvokeAsync(ctx, Next(null));

        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_When_HeaderInvalid()
    {
        var ctx = CreateContext("wrong-secret");
        var filter = new InternalSecretFilter();

        var result = await filter.InvokeAsync(ctx, Next(null));

        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task Should_InvokeNext_When_HeaderCorrect()
    {
        var ctx = CreateContext("super-secret");
        var filter = new InternalSecretFilter();

        var nextCalled = false;

        EndpointFilterDelegate next = _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>("ok");
        };

        var result = await filter.InvokeAsync(ctx, next);

        nextCalled.Should().BeTrue();
        result.Should().Be("ok");
    }
}
