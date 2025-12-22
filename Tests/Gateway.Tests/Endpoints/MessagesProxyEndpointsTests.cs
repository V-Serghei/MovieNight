using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MovieNight.Gateway.Endpoints;
using Xunit;

namespace Gateway.Tests.Endpoints;

public class MessagesProxyEndpointsTests
{
    [Fact]
    public async Task ListProxy_WithoutSenderId_CallsMessagesRoot()
    {
        // Arrange
        string? capturedPath   = null;
        string? capturedMethod = null;
        string? capturedAuth   = null;
        string? capturedCookie = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath   = req.RequestUri!.PathAndQuery;
            capturedAuth   = req.Headers.Authorization?.ToString();
            capturedCookie = req.Headers.TryGetValues("Cookie", out var cookies)
                ? string.Join(";", cookies)
                : null;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"items":[]}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://messages") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("messages")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path   = "/messages";
        ctx.Request.Headers["Authorization"] = "Bearer token";
        ctx.Response.Body = new MemoryStream();

        // Act
        await MessagesProxyEndpoints.ListProxy(ctx, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("GET");
        capturedPath.Should().Be("/messages");
        capturedAuth.Should().Be("Bearer token");
        // Cookie не передавали — его и нет
        capturedCookie.Should().BeNull();

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"items":[]}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task ListProxy_WithSenderId_CallsSentEndpoint()
    {
        // Arrange
        string? capturedPath   = null;
        string? capturedMethod = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath   = req.RequestUri!.PathAndQuery;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"items":[1,2]}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://messages") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("messages")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path   = "/messages";
        ctx.Request.QueryString = new QueryString("?senderId=user-123");
        ctx.Response.Body = new MemoryStream();

        // Act
        await MessagesProxyEndpoints.ListProxy(ctx, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("GET");
        capturedPath.Should().Be("/messages/sent/user-123");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"items":[1,2]}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task GetByIdProxy_ForwardsCorrectPath_AndAuth()
    {
        // Arrange
        string? capturedPath   = null;
        string? capturedMethod = null;
        string? capturedAuth   = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath   = req.RequestUri!.PathAndQuery;
            capturedAuth   = req.Headers.Authorization?.ToString();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"id":"msg-1"}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://messages") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("messages")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path   = "/messages";
        ctx.Request.Headers["Authorization"] = "Bearer abc";
        ctx.Response.Body = new MemoryStream();

        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        await MessagesProxyEndpoints.GetByIdProxy(ctx, factoryMock.Object, id, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("GET");
        capturedPath.Should().Be($"/messages/{id}");
        capturedAuth.Should().Be("Bearer abc");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"id":"msg-1"}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task GetByReceiverIdProxy_ForwardsReceiverId_AndAuth()
    {
        // Arrange
        string? capturedPath   = null;
        string? capturedMethod = null;
        string? capturedAuth   = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath   = req.RequestUri!.PathAndQuery;
            capturedAuth   = req.Headers.Authorization?.ToString();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"items":[]}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://messages") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("messages")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path   = "/messages/by-receiver";
        ctx.Request.Headers["Authorization"] = "Bearer token";
        ctx.Response.Body = new MemoryStream();

        var receiverId = "receiver-42";

        // Act
        await MessagesProxyEndpoints.GetByReceiverIdProxy(ctx, factoryMock.Object, receiverId, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("GET");
        capturedPath.Should().Be("/messages/by-receiver/receiver-42");
        capturedAuth.Should().Be("Bearer token");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"items":[]}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task CreateProxy_ForwardsBody_ContentType_AndAuth()
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
                Content = new StringContent("""{"id":"msg-1"}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://messages") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("messages")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method      = "POST";
        ctx.Request.Path        = "/messages/compose";
        ctx.Request.ContentType = "application/json; charset=utf-8";
        ctx.Request.Headers["Authorization"] = "Bearer xyz";

        var bodyJson = """{"to":"user-1","text":"Hello"}""";
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(bodyJson));
        ms.Position = 0;
        ctx.Request.Body = ms;
        ctx.Request.ContentLength = ms.Length;
        ctx.Response.Body = new MemoryStream();

        // Act
        await MessagesProxyEndpoints.CreateProxy(ctx, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("POST");
        capturedPath.Should().Be("/messages/compose");
        capturedAuth.Should().Be("Bearer xyz");
        capturedContentType.Should().StartWith("application/json");
        capturedBody.Should().Be(bodyJson);

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"id":"msg-1"}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task CreateProxy_SetsDefaultContentType_WhenMissing()
    {
        // Arrange
        string? capturedContentType = null;

        var handler = new StubHandler(req =>
        {
            capturedContentType = req.Content?.Headers.ContentType?.ToString();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"ok":true}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://messages") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("messages")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "POST";
        ctx.Request.Path   = "/messages/compose";
        // ContentType = null
        var bodyJson = """{"text":"hi"}""";
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(bodyJson));
        ms.Position = 0;
        ctx.Request.Body = ms;
        ctx.Request.ContentLength = ms.Length;
        ctx.Response.Body = new MemoryStream();

        // Act
        await MessagesProxyEndpoints.CreateProxy(ctx, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task ProxyAny_ForwardsMethod_Path_Query_AndBody()
    {
        // Arrange
        string? capturedMethod = null;
        string? capturedPath   = null;
        string? capturedBody   = null;
        string? capturedAuth   = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath   = req.RequestUri!.PathAndQuery;
            capturedBody   = req.Content is null
                ? null
                : req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            capturedAuth   = req.Headers.Authorization?.ToString();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"ok":true}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://messages") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("messages")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method      = "PATCH";
        ctx.Request.Path        = "/messages/ignored";
        ctx.Request.QueryString = new QueryString("?page=2");
        ctx.Request.Headers["Authorization"] = "Bearer abc";

        var bodyJson = """{"read":true}""";
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(bodyJson));
        ms.Position = 0;
        ctx.Request.Body = ms;
        ctx.Request.ContentLength = ms.Length;
        ctx.Response.Body = new MemoryStream();

        var pathParam = "thread/123/messages/5";

        // Act
        await MessagesProxyEndpoints.ProxyAny(ctx, factoryMock.Object, pathParam, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("PATCH");
        capturedPath.Should().Be("/messages/thread/123/messages/5?page=2");
        capturedBody.Should().Be(bodyJson);
        capturedAuth.Should().Be("Bearer abc");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"ok":true}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public void CopyAuthOrCookie_UsesAuthorization_WhenPresent()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Authorization"] = "Bearer abc";
        ctx.Request.Headers["Cookie"] = "auth=xyz";

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/messages");

        // Act
        MessagesProxyEndpoints.CopyAuthOrCookie(ctx, msg);

        // Assert
        msg.Headers.Authorization!.ToString().Should().Be("Bearer abc");
        msg.Headers.TryGetValues("Cookie", out var cookies).Should().BeTrue();
        string.Join(";", cookies!).Should().Contain("auth=xyz");
    }

    [Fact]
    public void CopyAuthOrCookie_FallsBackToCookie_WhenNoAuthorization()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Cookie"] = "sid=123; auth=xyz";

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/messages");

        // Act
        MessagesProxyEndpoints.CopyAuthOrCookie(ctx, msg);

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
        await MessagesProxyEndpoints.ProxyCopyResponse(ctx, resp, CancellationToken.None);

        // Assert
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        ctx.Response.Headers["X-Test"].Should().Contain("123");
        ctx.Response.Headers["X-Content"].Should().Contain("abc");
        ctx.Response.ContentType.Should().StartWith("text/plain");

        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be("hello");
    }

    // Вспомогательный HttpMessageHandler
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
