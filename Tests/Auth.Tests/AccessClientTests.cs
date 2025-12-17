using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Auth.Infrastructure.Clients;
using FluentAssertions;
using Xunit;

namespace Auth.Tests;

public class AccessClientTests
{
    private static HttpClient CreateClient(HttpStatusCode status, object? body)
    {
        var handler = new StubHandler((req, ct) =>
        {
            var resp = new HttpResponseMessage(status);
            if (body != null) resp.Content = JsonContent.Create(body);
            return Task.FromResult(resp);
        });

        return new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
    }

    [Fact]
    public async Task GetPrimaryRoleAsync_Should_ReturnAdmin_WhenAdminPresent()
    {
        var http = CreateClient(HttpStatusCode.OK, new AccessClient.RolesResponse(["user", "admin"]));
        var client = new AccessClient(http);

        var result = await client.GetPrimaryRoleAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().Be("admin");
    }

    [Fact]
    public async Task GetPrimaryRoleAsync_Should_ReturnUser_WhenUserPresent()
    {
        var http = CreateClient(HttpStatusCode.OK, new AccessClient.RolesResponse(["user"]));
        var client = new AccessClient(http);

        var result = await client.GetPrimaryRoleAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().Be("user");
    }

    [Fact]
    public async Task GetPrimaryRoleAsync_Should_ReturnFirstRole_WhenNoAdminOrUser()
    {
        var http = CreateClient(HttpStatusCode.OK, new AccessClient.RolesResponse(["moderator"]));
        var client = new AccessClient(http);

        var result = await client.GetPrimaryRoleAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().Be("moderator");
    }

    [Fact]
    public async Task GetPrimaryRoleAsync_Should_ReturnNull_WhenNoRoles()
    {
        var http = CreateClient(HttpStatusCode.OK, new AccessClient.RolesResponse([]));
        var client = new AccessClient(http);

        var result = await client.GetPrimaryRoleAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPrimaryRoleAsync_Should_ReturnNull_WhenErrorStatus()
    {
        var http = CreateClient(HttpStatusCode.NotFound, null);
        var client = new AccessClient(http);

        var result = await client.GetPrimaryRoleAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _handler(request, cancellationToken);
    }
}
