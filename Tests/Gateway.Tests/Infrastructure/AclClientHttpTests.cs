using MovieNight.Gateway.Infrastructure;

namespace Gateway.Tests.Infrastructure;

using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

public class AclClientHttpTests
{
    [Fact]
    public async Task IsAllowedAsync_ReturnsFalse_WhenStatusNotSuccess()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://access") };
        var client = new AclClientHttp(http);

        var allowed = await client.IsAllowedAsync(Guid.NewGuid(), "/foo", "GET");

        Assert.False(allowed);
    }

    [Fact]
    public async Task IsAllowedAsync_ParsesAllowedProperty()
    {
        var json = JsonSerializer.Serialize(new { allowed = true });
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var http = new HttpClient(handler) { BaseAddress = new Uri("http://access") };
        var client = new AclClientHttp(http);

        var userId = Guid.NewGuid();
        var allowed = await client.IsAllowedAsync(userId, "/foo", "GET");

        Assert.True(allowed);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
            => _handler = handler;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_handler(request));
    }
}
