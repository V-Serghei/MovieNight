using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MovieNight.Gateway.Endpoints;
using Xunit;

namespace Gateway.Tests.Endpoints;

public class BookmarksProxyEndpointsTests
{
    [Fact]
    public async Task ProxyPassthrough_Forwards_Method_Path_Query_And_Headers_And_User()
    {
        string? capturedMethod = null;
        string? capturedPathAndQuery = null;
        string? capturedAuth = null;
        string[]? capturedCookies = null;
        string[]? capturedUserIds = null;
        string[]? capturedRoles = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPathAndQuery = req.RequestUri!.PathAndQuery;
            capturedAuth = req.Headers.Authorization?.ToString();
            capturedCookies = req.Headers.TryGetValues("Cookie", out var c) ? c.ToArray() : Array.Empty<string>();
            capturedUserIds = req.Headers.TryGetValues("X-UserId", out var u) ? u.ToArray() : Array.Empty<string>();
            capturedRoles = req.Headers.TryGetValues("X-UserRole", out var r) ? r.ToArray() : Array.Empty<string>();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"ok":true}""", Encoding.UTF8, "application/json")
            };
        });

        var backendClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://bookmarks")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("bookmarks")).Returns(backendClient);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path = "/bookmarks";
        ctx.Request.QueryString = new QueryString("?kind=temp");
        ctx.Request.Headers["Authorization"] = "Bearer token";
        ctx.Request.Headers["Cookie"] = "auth=xyz";

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Role, "member")
        }, "test");
        ctx.User = new ClaimsPrincipal(identity);

        ctx.Response.Body = new MemoryStream();

        await BookmarksProxyEndpoints.ProxyPassthrough(ctx, factoryMock.Object, CancellationToken.None);

        capturedMethod.Should().Be("GET");
        capturedPathAndQuery.Should().Be("/bookmarks?kind=temp");
        capturedAuth.Should().Be("Bearer token");
        capturedCookies!.Should().BeEmpty();

        capturedUserIds.Should().Contain("user-123");
        capturedRoles.Should().Contain("member");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be("""{"ok":true}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task ProxyWithBody_Forwards_Body_Path_And_UserHeaders()
    {
        string? capturedMethod = null;
        string? capturedPathAndQuery = null;
        string? capturedBody = null;
        string[]? capturedUserIds = null;
        string[]? capturedRoles = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPathAndQuery = req.RequestUri!.PathAndQuery;
            capturedBody = req.Content is null ? null : req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            capturedUserIds = req.Headers.TryGetValues("X-UserId", out var u) ? u.ToArray() : Array.Empty<string>();
            capturedRoles = req.Headers.TryGetValues("X-UserRole", out var r) ? r.ToArray() : Array.Empty<string>();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"created":true}""", Encoding.UTF8, "application/json")
            };
        });

        var backendClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://bookmarks")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("bookmarks")).Returns(backendClient);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "POST";
        ctx.Request.Path = "/bookmarks/watched";
        ctx.Request.QueryString = new QueryString("?source=client");
        ctx.Request.ContentType = "application/json";

        var json = """{"movieId":"11111111-1111-1111-1111-111111111111","kind":"Watched"}""";
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
        ms.Position = 0;
        ctx.Request.Body = ms;
        ctx.Request.ContentLength = ms.Length;

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-999"),
            new Claim(ClaimTypes.Role, "admin")
        }, "test");
        ctx.User = new ClaimsPrincipal(identity);

        ctx.Response.Body = new MemoryStream();

        await BookmarksProxyEndpoints.ProxyWithBody(ctx, factoryMock.Object, CancellationToken.None);

        capturedMethod.Should().Be("POST");
        capturedPathAndQuery.Should().Be("/bookmarks/watched?source=client?source=client");
        capturedBody.Should().Be(json);
        capturedUserIds.Should().Contain("user-999");
        capturedRoles.Should().Contain("admin");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"created":true}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public void CopyCookie_CopiesCookieHeader()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Cookie"] = "sid=123; theme=dark";
        using var msg = new HttpRequestMessage(HttpMethod.Get, "/bookmarks");

        BookmarksProxyEndpoints.CopyCookie(ctx, msg);

        msg.Headers.TryGetValues("Cookie", out var cookies).Should().BeTrue();
        string.Join(";", cookies!).Should().Contain("sid=123");
    }

    [Fact]
    public void CopyAuthOrCookie_UsesAuthorization_WhenPresent()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Authorization"] = "Bearer abc";
        ctx.Request.Headers["Cookie"] = "sid=123";

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/bookmarks");

        BookmarksProxyEndpoints.CopyAuthOrCookie(ctx, msg);

        msg.Headers.Authorization!.ToString().Should().Be("Bearer abc");
        msg.Headers.TryGetValues("Cookie", out _).Should().BeFalse();
    }

    [Fact]
    public void CopyAuthOrCookie_FallsBackToCookie_WhenNoAuthorization()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Cookie"] = "sid=123; auth=xyz";

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/bookmarks");

        BookmarksProxyEndpoints.CopyAuthOrCookie(ctx, msg);

        msg.Headers.Authorization.Should().BeNull();
        msg.Headers.TryGetValues("Cookie", out var cookies).Should().BeTrue();
        string.Join(";", cookies!).Should().Contain("auth=xyz");
    }

    [Fact]
    public async Task ProxyCopyResponse_Copies_Status_Headers_And_Body()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        var resp = new HttpResponseMessage(HttpStatusCode.Accepted);
        resp.Headers.Add("X-Test", "123");
        resp.Content = new StringContent("hello", Encoding.UTF8, "text/plain");
        resp.Content.Headers.Add("X-Content", "abc");
        resp.Headers.TransferEncodingChunked = true;

        await BookmarksProxyEndpoints.ProxyCopyResponse(ctx, resp, CancellationToken.None);

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        ctx.Response.Headers["X-Test"].Should().Contain("123");
        ctx.Response.Headers["X-Content"].Should().Contain("abc");
        ctx.Response.Headers.ContainsKey("transfer-encoding").Should().BeFalse();

        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be("hello");
        ctx.Response.ContentType.Should().StartWith("text/plain");
    }

    [Fact]
    public void AddUserHeaders_Adds_UserId_And_Role_WhenPresent()
    {
        var ctx = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-42"),
            new Claim(ClaimTypes.Role, "moderator")
        }, "test");
        ctx.User = new ClaimsPrincipal(identity);

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/bookmarks");

        BookmarksProxyEndpoints.AddUserHeaders(ctx, msg);

        msg.Headers.TryGetValues("X-UserId", out var ids).Should().BeTrue();
        ids.Should().Contain("user-42");
        msg.Headers.TryGetValues("X-UserRole", out var roles).Should().BeTrue();
        roles.Should().Contain("moderator");
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) => _handler = handler;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_handler(request));
    }
}
