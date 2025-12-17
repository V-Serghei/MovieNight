using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MovieNight.Gateway.Endpoints;
using Xunit;

namespace Gateway.Tests.Endpoints;

public class MediaProxyEndpointsTests
{
    [Fact]
    public async Task UploadProxy_ForwardsToMediaRoot_WithSameMethod()
    {
        // Arrange
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

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("uploaded", Encoding.UTF8, "text/plain")
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://media") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("media")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method      = "POST";
        ctx.Request.Path        = "/media";
        ctx.Request.ContentType = "multipart/form-data; boundary=xyz";

        var body = "--xyz\r\ncontent\r\n--xyz--";
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(body));
        ms.Position = 0;
        ctx.Request.Body         = ms;
        ctx.Request.ContentLength = ms.Length;

        ctx.Response.Body = new MemoryStream();

        // Act
        await MediaProxyEndpoints.UploadProxy(ctx, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("POST");
        capturedPath.Should().Be("/media");
        capturedBody.Should().Be(body);

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("uploaded");
        ctx.Response.ContentType.Should().StartWith("text/plain");
    }

    [Fact]
    public async Task DownloadProxy_ForwardsGet_WithId_AndAuth()
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

            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Content = new ByteArrayContent(Encoding.UTF8.GetBytes("file-bytes"));
            resp.Content.Headers.ContentType = new("application/octet-stream");
            return resp;
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://media") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("media")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path   = "/media";
        ctx.Request.Headers["Authorization"] = "Bearer token";
        ctx.Response.Body = new MemoryStream();

        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        await MediaProxyEndpoints.DownloadProxy(ctx, id, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("GET");
        capturedPath.Should().Be($"/media/{id}");
        capturedAuth.Should().Be("Bearer token");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var bytes = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        bytes.Should().Be("file-bytes");
        ctx.Response.ContentType.Should().StartWith("application/octet-stream");
    }

    [Fact]
    public async Task InfoProxy_ForwardsGet_InfoPath_AndAuth()
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
                Content = new StringContent("""{"id":"media-1"}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://media") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("media")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path   = "/media";
        ctx.Request.Headers["Authorization"] = "Bearer token";
        ctx.Response.Body = new MemoryStream();

        var id = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Act
        await MediaProxyEndpoints.InfoProxy(ctx, id, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("GET");
        capturedPath.Should().Be($"/media/{id}/info");
        capturedAuth.Should().Be("Bearer token");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"id":"media-1"}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task DeleteProxy_ForwardsDelete_WithId_AndAuth()
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

            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://media") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("media")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "DELETE";
        ctx.Request.Path   = "/media";
        ctx.Request.Headers["Authorization"] = "Bearer token";
        ctx.Response.Body = new MemoryStream();

        var id = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Act
        await MediaProxyEndpoints.DeleteProxy(ctx, id, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("DELETE");
        capturedPath.Should().Be($"/media/{id}");
        capturedAuth.Should().Be("Bearer token");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public void CopyCookie_CopiesCookieHeader()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Cookie"] = "sid=123; theme=dark";
        using var msg = new HttpRequestMessage(HttpMethod.Get, "/media");

        // Act
        MediaProxyEndpoints.CopyCookie(ctx, msg);

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
        ctx.Request.Headers["Cookie"]       = "sid=123";

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/media");

        // Act
        MediaProxyEndpoints.CopyAuthOrCookie(ctx, msg);

        // Assert
        msg.Headers.Authorization!.ToString().Should().Be("Bearer abc");
        // Cookie не добавляем через CopyAuthOrCookie при наличии Authorization
        msg.Headers.TryGetValues("Cookie", out _).Should().BeFalse();
    }

    [Fact]
    public void CopyAuthOrCookie_FallsBackToCookie_WhenNoAuthorization()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Cookie"] = "sid=123; auth=xyz";

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/media");

        // Act
        MediaProxyEndpoints.CopyAuthOrCookie(ctx, msg);

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
        await MediaProxyEndpoints.ProxyCopyResponse(ctx, resp, CancellationToken.None);

        // Assert
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        ctx.Response.Headers["X-Test"].Should().Contain("123");
        ctx.Response.Headers["X-Content"].Should().Contain("abc");
        ctx.Response.ContentType.Should().StartWith("text/plain");

        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be("hello");
    }

    // вспомогательный handler для перехвата исходящих запросов
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
