using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MovieNight.Gateway.Endpoints;
using Xunit;

namespace Gateway.Tests.Endpoints;

public class ReviewProxyEndpointsTests
{
    [Fact]
    public async Task GetByFilmIdProxy_ForwardsRequestAndCopiesResponse()
    {
        // Arrange
        var filmId = "tt123";
        var backendBody = """{"ok":true}""";

        string? capturedPath = null;
        string? capturedAuth = null;
        string? capturedCookie = null;

        var handler = new StubHandler(req =>
        {
            capturedPath = req.RequestUri!.PathAndQuery;
            capturedAuth = req.Headers.Authorization?.ToString();
            capturedCookie = req.Headers.TryGetValues("Cookie", out var cookies)
                ? string.Join(";", cookies)
                : null;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(backendBody, Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://review")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(x => x.CreateClient("review")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Authorization"] = "Bearer test-token";
        ctx.Request.Headers["Cookie"] = "auth=xyz";
        ctx.Response.Body = new MemoryStream();

        // Act
        await ReviewProxyEndpoints.GetByFilmIdProxy(ctx, factoryMock.Object, filmId, CancellationToken.None);

        // Assert: проверяем, что запрос ушёл как надо
        capturedPath.Should().Be("/review/tt123");
        capturedAuth.Should().Be("Bearer test-token");
        capturedCookie.Should().Contain("auth=xyz");
        
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be(backendBody);

        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task CreateReviewProxy_ForwardsBody_AndContentType()
    {
        // Arrange
        var requestJson = """{"text":"hello"}""";
        string? capturedPath = null;
        string? capturedMethod = null;
        string? capturedContentType = null;
        string? capturedBody = null;

        var handler = new StubHandler(req =>
        {
            capturedPath = req.RequestUri!.PathAndQuery;
            capturedMethod = req.Method.Method;
            capturedContentType = req.Content?.Headers.ContentType?.ToString();

            capturedBody = req.Content is null
                ? null
                : req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"id":1}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://review")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(x => x.CreateClient("review")).Returns(client);

        var ctx = new DefaultHttpContext();
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));
        ms.Position = 0;
        ctx.Request.Body = ms;
        ctx.Request.ContentType = "application/json; charset=utf-8";
        ctx.Response.Body = new MemoryStream();

        // Act
        await ReviewProxyEndpoints.CreateReviewProxy(ctx, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedPath.Should().Be("/review");
        capturedMethod.Should().Be("POST");
        capturedContentType.Should().Be("application/json; charset=utf-8");
        capturedBody.Should().Be(requestJson);

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        ctx.Response.Body.Position = 0;
        var responseBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        responseBody.Should().Be("""{"id":1}""");
    }

    [Fact]
    public async Task ProxyAny_ForwardsMethod_Path_Query_AndBody()
    {
        // Arrange
        var pathParam = "films/42/comments";
        string? capturedPathAndQuery = null;
        string? capturedMethod = null;
        string? capturedBody = null;

        var handler = new StubHandler(req =>
        {
            capturedPathAndQuery = req.RequestUri!.PathAndQuery;
            capturedMethod = req.Method.Method;
            capturedBody = req.Content is null
                ? null
                : req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"ok":true}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://review")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(x => x.CreateClient("review")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "PATCH";
        ctx.Request.QueryString = new QueryString("?page=2");
        var bodyJson = """{"text":"patched"}""";
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(bodyJson));
        ms.Position = 0;
        ctx.Request.Body = ms;
        ctx.Request.ContentLength = ms.Length;
        ctx.Response.Body = new MemoryStream();

        // Act
        await ReviewProxyEndpoints.ProxyAny(ctx, factoryMock.Object, pathParam, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("PATCH");
        capturedPathAndQuery.Should().Be("/review/films/42/comments?page=2");
        capturedBody.Should().Be(bodyJson);

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var responseBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        responseBody.Should().Be("""{"ok":true}""");
    }

    [Fact]
    public void CopyAuthOrCookie_CopiesHeaders()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Authorization"] = "Bearer abc";
        ctx.Request.Headers["Cookie"] = "sid=123; theme=dark";

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/review/test");

        // Act
        ReviewProxyEndpoints.CopyAuthOrCookie(ctx, msg);

        // Assert
        msg.Headers.Authorization!.ToString().Should().Be("Bearer abc");
        msg.Headers.TryGetValues("Cookie", out var cookies).Should().BeTrue();
        string.Join(";", cookies!).Should().Contain("sid=123");
    }

    [Fact]
    public void AddUserHeaders_AddsUserInfoFromClaims()
    {
        // Arrange
        var ctx = new DefaultHttpContext();

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(ClaimTypes.Name, "Sergey"),
            new Claim(ClaimTypes.Email, "sergey@example.com")
        }, "test");

        ctx.User = new ClaimsPrincipal(identity);

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/review");

        // Act
        ReviewProxyEndpoints.AddUserHeaders(ctx, msg);

        // Assert
        msg.Headers.GetValues("X-UserId").Single().Should().Be("user-123");
        msg.Headers.GetValues("X-UserRole").Single().Should().Be("admin");
        msg.Headers.GetValues("X-UserName").Single().Should().Be("Sergey");
    }

    // Вспомогательный HttpMessageHandler, чтобы перехватывать запросы
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
