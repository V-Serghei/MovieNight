using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Gateway.Tests.Endpoints;

public class InternalAccessProxyEndpointsTests
{
    [Fact]
    public async Task GetUserRoles_ProxiesRequest_ToAccessService()
    {
        // Arrange: перехватываем исходящий запрос к сервису access
        string? capturedMethod = null;
        string? capturedPath   = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath   = req.RequestUri!.PathAndQuery;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"roles":["admin","user"]}""", Encoding.UTF8, "application/json")
            };
        });

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Подменяем named HttpClient "access"
                    services.AddHttpClient("access")
                        .ConfigurePrimaryHttpMessageHandler(() => handler);
                });
            });

        var client = factory.CreateClient();

        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/_internal/access/users/{userId}/roles");
        // InternalSecretFilter пропускает с этим значением
        request.Headers.Add("X-Internal-Secret", "dev-internal-secret");

        // Act
        var resp = await client.SendAsync(request);

        // Assert: что ушло в Access.Service
        capturedMethod.Should().Be("GET");
        capturedPath.Should().Be($"/access/users/{userId}/roles");

        // И что вернулось наружу
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var respBody = await resp.Content.ReadAsStringAsync();
        respBody.Should().Be("""{"roles":["admin","user"]}""");
        resp.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetUserRoles_Returns401_WithoutInternalSecret()
    {
        // Arrange: обычная фабрика, без подмены клиентов
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var userId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Без заголовка X-Internal-Secret
        var resp = await client.GetAsync($"/_internal/access/users/{userId}/roles");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Вспомогательный handler для перехвата исходящих запросов
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
            => _handler = handler;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(_handler(request));
    }
}
