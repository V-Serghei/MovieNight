using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Gateway.Tests.Endpoints;

public class InternalUsersProxyEndpointsTests
{
    [Fact]
    public async Task VerifyCredentials_ProxiesRequest_ToUsersService()
    {
        // Arrange: перехватываем исходящий запрос к сервису users
        string? capturedMethod = null;
        string? capturedPath   = null;
        string? capturedBody   = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath   = req.RequestUri!.PathAndQuery;
            capturedBody   = req.Content is null
                ? null
                : req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"valid":true}""", Encoding.UTF8, "application/json")
            };
        });

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Подменяем named HttpClient "users"
                    services.AddHttpClient("users")
                        .ConfigurePrimaryHttpMessageHandler(() => handler);
                });
            });

        var client = factory.CreateClient();

        var json = """{"userName":"test","password":"secret"}""";
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/_internal/users/verify-credentials");
        request.Headers.Add("X-Internal-Secret", "dev-internal-secret"); // фильтр пропустит
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var resp = await client.SendAsync(request);

        // Assert: что ушло в Users.Service
        capturedMethod.Should().Be("POST");
        capturedPath.Should().Be("/users/verify-credentials");
        capturedBody.Should().Be(json);

        // и что вернулось наружу
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var respBody = await resp.Content.ReadAsStringAsync();
        respBody.Should().Be("""{"valid":true}""");
        resp.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetUserById_ProxiesRequest_ToUsersService()
    {
        // Arrange
        string? capturedMethod = null;
        string? capturedPath   = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath   = req.RequestUri!.PathAndQuery;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"id":"user-1"}""", Encoding.UTF8, "application/json")
            };
        });

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddHttpClient("users")
                        .ConfigurePrimaryHttpMessageHandler(() => handler);
                });
            });

        var client = factory.CreateClient();

        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/_internal/users/{userId}");
        request.Headers.Add("X-Internal-Secret", "dev-internal-secret");

        // Act
        var resp = await client.SendAsync(request);

        // Assert: что ушло в Users.Service
        capturedMethod.Should().Be("GET");
        capturedPath.Should().Be($"/users/{userId}");

        // И ответ наружу
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var respBody = await resp.Content.ReadAsStringAsync();
        respBody.Should().Be("""{"id":"user-1"}""");
        resp.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task InternalEndpoints_Return401_WithoutSecret()
    {
        // Этот тест проверяет, что InternalSecretFilter реально работает
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var userId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Нет заголовка X-Internal-Secret
        var resp = await client.GetAsync($"/_internal/users/{userId}");

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Вспомогательный handler для подмены backend'а users
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
