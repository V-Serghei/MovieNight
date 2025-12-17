using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MovieNight.Gateway.Endpoints;
using Xunit;

namespace Gateway.Tests.Endpoints;

public class FriendsProxyEndpointsTests
{
    [Fact]
    public async Task GetByUserIdProxy_ForwardsToBackend_WithAuth()
    {
        // Arrange
        string? capturedMethod = null;
        string? capturedPath   = null;
        string? capturedAuth   = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath   = req.RequestUri!.PathAndQuery;
            capturedAuth   = req.Headers.Authorization?.ToString();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"friends":[]}""", Encoding.UTF8, "application/json")
            };
        });

        var backendClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://friends")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("friends")).Returns(backendClient);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path   = "/friends/user-123";
        ctx.Request.Headers["Authorization"] = "Bearer token";
        ctx.Response.Body = new MemoryStream();

        // Act
        await FriendsProxyEndpoints.GetByUserIdProxy(ctx, "user-123", factoryMock.Object, CancellationToken.None);

        // Assert: что ушло на backend
        capturedMethod.Should().Be("GET");
        capturedPath.Should().Be("/friends/user-123");
        capturedAuth.Should().Be("Bearer token");

        // Ответ клиенту
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"friends":[]}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task CreateFriendProxy_SerializesBody_AndUsesPost()
    {
        // Arrange
        string? capturedMethod      = null;
        string? capturedPath        = null;
        string? capturedAuth        = null;
        string? capturedContentType = null;
        string? capturedBody        = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod      = req.Method.Method;
            capturedPath        = req.RequestUri!.PathAndQuery;
            capturedAuth        = req.Headers.Authorization?.ToString();
            capturedContentType = req.Content?.Headers.ContentType?.ToString();
            capturedBody        = req.Content is null
                ? null
                : req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"id":"rel-1"}""", Encoding.UTF8, "application/json")
            };
        });

        var backendClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://friends")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("friends")).Returns(backendClient);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "POST";
        ctx.Request.Path   = "/friends";
        ctx.Request.Headers["Authorization"] = "Bearer abc";
        ctx.Response.Body = new MemoryStream();

        var body = new
        {
            userId   = "user-1",
            friendId = "user-2"
        };

        // Act
        await FriendsProxyEndpoints.CreateFriendProxy(ctx, body, factoryMock.Object, CancellationToken.None);

        // Assert: исходящий запрос
        capturedMethod.Should().Be("POST");
        capturedPath.Should().Be("/friends");
        capturedAuth.Should().Be("Bearer abc");
        capturedContentType.Should().StartWith("application/json");

        capturedBody.Should().NotBeNull();
        using (var doc = JsonDocument.Parse(capturedBody!))
        {
            var root = doc.RootElement;
            root.GetProperty("userId").GetString().Should().Be("user-1");
            root.GetProperty("friendId").GetString().Should().Be("user-2");
        }

        // Ответ клиенту
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"id":"rel-1"}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task ProxyAny_ForwardsMethod_Path_Query_Body_AndHeaders()
    {
        // Arrange
        string?   capturedMethod      = null;
        string?   capturedPath        = null;
        string?   capturedBody        = null;
        string?   capturedAuth        = null;
        string[]? capturedCustomHeader = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath   = req.RequestUri!.PathAndQuery;
            capturedBody   = req.Content is null
                ? null
                : req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            capturedAuth = req.Headers.Authorization?.ToString();
            capturedCustomHeader = req.Headers.TryGetValues("X-Trace", out var vals)
                ? vals.ToArray()
                : Array.Empty<string>();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"ok":true}""", Encoding.UTF8, "application/json")
            };
        });

        var backendClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://friends")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("friends")).Returns(backendClient);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method      = "PATCH";
        ctx.Request.Path        = "/friends/something";
        ctx.Request.QueryString = new QueryString("?page=2");
        ctx.Request.Headers["Authorization"] = "Bearer xyz";
        ctx.Request.Headers["X-Trace"]       = "trace-123";
        ctx.Request.ContentType             = "application/json";

        var jsonBody = """{"note":"hello"}""";
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonBody));
        ms.Position = 0;
        ctx.Request.Body          = ms;
        ctx.Request.ContentLength = ms.Length;

        ctx.Response.Body = new MemoryStream();

        var pathParam = "user-1/block";

        // Act
        await FriendsProxyEndpoints.ProxyAny(pathParam, ctx, factoryMock.Object, CancellationToken.None);

        // Assert: что ушло на backend
        capturedMethod.Should().Be("PATCH");
        capturedPath.Should().Be("/friends/user-1/block?page=2");
        capturedBody.Should().Be(jsonBody);
        capturedAuth.Should().Be("Bearer xyz");
        capturedCustomHeader.Should().Contain("trace-123");

        // Ответ клиенту
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"ok":true}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public void CopyCookie_CopiesCookieHeader()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Cookie"] = "sid=123; theme=dark";
        using var msg = new HttpRequestMessage(HttpMethod.Get, "/friends");

        // Act
        FriendsProxyEndpoints.CopyCookie(ctx, msg);

        // Assert
        msg.Headers.TryGetValues("Cookie", out var cookies).Should().BeTrue();
        string.Join(";", cookies!).Should().Contain("sid=123");
    }

    [Fact]
    public void CopyAuthOrCookie_UsesAuthorization_WhenPresent()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Authorization"] = "Bearer abc";
        ctx.Request.Headers["Cookie"]       = "sid=123"; // не должно использоваться

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/friends");

        // Act
        FriendsProxyEndpoints.CopyAuthOrCookie(ctx, msg);

        // Assert
        msg.Headers.Authorization!.ToString().Should().Be("Bearer abc");
        msg.Headers.TryGetValues("Cookie", out _).Should().BeFalse();
    }

    [Fact]
    public void CopyAuthOrCookie_FallsBackToCookie_WhenNoAuthorization()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Cookie"] = "sid=123; auth=xyz";

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/friends");

        // Act
        FriendsProxyEndpoints.CopyAuthOrCookie(ctx, msg);

        // Assert
        msg.Headers.Authorization.Should().BeNull();
        msg.Headers.TryGetValues("Cookie", out var cookies).Should().BeTrue();
        string.Join(";", cookies!).Should().Contain("auth=xyz");
    }

    [Fact]
    public async Task ProxyCopyResponse_CopiesStatus_Headers_AndBody()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        var resp = new HttpResponseMessage(HttpStatusCode.Accepted);
        resp.Headers.Add("X-Test", "123");
        resp.Content = new StringContent("hello", Encoding.UTF8, "text/plain");
        resp.Content.Headers.Add("X-Content", "abc");

        // Act
        await FriendsProxyEndpoints.ProxyCopyResponse(ctx, resp, CancellationToken.None);

        // Assert
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        ctx.Response.Headers["X-Test"].Should().Contain("123");
        ctx.Response.Headers["X-Content"].Should().Contain("abc");
        ctx.Response.ContentType.Should().StartWith("text/plain");

        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be("hello");
    }

    // Вспомогательный HttpMessageHandler для перехвата исходящих запросов
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
