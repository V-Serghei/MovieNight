using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MovieNight.Gateway.Endpoints;
using Xunit;

namespace Gateway.Tests.Endpoints;

public class UsersPublicEndpoints_GetMeBasicTests
{
    [Fact]
    public async Task GetMeBasic_Returns_User_With_Roles()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // --- фейковый /auth/me ---
        var authJson = $$"""
        {
          "user": {
            "id": "{{userId}}",
            "email": "test@example.com",
            "displayName": "Test User"
          }
        }
        """;

        var authHandler = new StubHandler(_ =>
        {
            // Можно при желании проверить, что реально ходим на /auth/me
            // _.RequestUri!.PathAndQuery.Should().Be("/auth/me");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(authJson, Encoding.UTF8, "application/json")
            };
        });

        var authClient = new HttpClient(authHandler)
        {
            BaseAddress = new Uri("http://auth")
        };

        // --- фейковый /access/users/{id}/roles ---
        var rolesJson = """{ "roles": ["admin", "user"] }""";

        var accessHandler = new StubHandler(_ =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(rolesJson, Encoding.UTF8, "application/json")
            };
        });

        var accessClient = new HttpClient(accessHandler)
        {
            BaseAddress = new Uri("http://access")
        };

        // --- IHttpClientFactory, который вернёт наши клиенты ---
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(x => x.CreateClient("auth")).Returns(authClient);
        factoryMock.Setup(x => x.CreateClient("access")).Returns(accessClient);

        // HttpContext с памятью в Response.Body
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        // Act
        await UsersPublicEndpoints.GetMeBasic(ctx, factoryMock.Object, CancellationToken.None);

        // Assert
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var json = await new StreamReader(ctx.Response.Body).ReadToEndAsync();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("id").GetGuid().Should().Be(userId);
        root.GetProperty("email").GetString().Should().Be("test@example.com");
        root.GetProperty("displayName").GetString().Should().Be("Test User");

        var roles = root.GetProperty("roles").EnumerateArray().Select(e => e.GetString()).ToArray();
        roles.Should().BeEquivalentTo(new[] { "admin", "user" });
    }

    /// <summary>
    /// Простой HttpMessageHandler, который отдаёт ответ, который мы ему даём.
    /// </summary>
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
            => _handler = handler;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_handler(request));
    }
}
