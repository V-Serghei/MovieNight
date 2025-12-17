using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using MovieNight.Gateway.DTO.User;
using MovieNight.Gateway.Endpoints;
using Xunit;

namespace Gateway.Tests.Endpoints;

public class AuthProxyEndpointsTests
{
    [Fact]
    public async Task LoginProxy_ForwardsBody_AndCopiesResponse()
    {
        string? capturedMethod = null;
        string? capturedPath = null;
        string? capturedBody = null;
        string? capturedContentType = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath = req.RequestUri!.PathAndQuery;
            capturedBody = req.Content is null ? null : req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            capturedContentType = req.Content?.Headers.ContentType?.ToString();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"token":"abc"}""", Encoding.UTF8, "application/json")
            };
        });

        var authClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://auth")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("auth")).Returns(authClient);

        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        await AuthProxyEndpoints.LoginProxy(ctx, default(LoginRequest)!, factoryMock.Object, CancellationToken.None);

        capturedMethod.Should().Be("POST");
        capturedPath.Should().Be("/auth/login");
        capturedBody.Should().Be("null");
        capturedContentType.Should().StartWith("application/json");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"token":"abc"}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task RegisterProxy_PropagatesErrorFromUsers_AndDoesNotCallAccess()
    {
        var regJson = """{"error":"invalid"}""";
        bool accessCalled = false;

        var usersHandler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(regJson, Encoding.UTF8, "application/json")
        });

        var accessHandler = new StubHandler(_ =>
        {
            accessCalled = true;
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });

        var usersClient = new HttpClient(usersHandler) { BaseAddress = new Uri("http://users") };
        var accessClient = new HttpClient(accessHandler) { BaseAddress = new Uri("http://access") };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("users")).Returns(usersClient);
        factoryMock.Setup(f => f.CreateClient("access")).Returns(accessClient);

        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Access:DefaultUserRoleId"] = Guid.NewGuid().ToString()
            }!)
            .Build();

        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        await AuthProxyEndpoints.RegisterProxy(ctx, default(RegisterRequest)!, factoryMock.Object, cfg, CancellationToken.None);

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be(regJson);
        ctx.Response.ContentType.Should().StartWith("application/json");
        accessCalled.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterProxy_OnSuccessWithId_LinksRoleAndReturns201()
    {
        var userId = Guid.NewGuid();
        var regJson = $$"""{"id":"{{userId}}","email":"test@example.com"}""";
        bool accessCalled = false;
        string? capturedAccessMethod = null;
        string? capturedAccessPath = null;

        var usersHandler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(regJson, Encoding.UTF8, "application/json")
        });

        var roleId = Guid.NewGuid();

        var accessHandler = new StubHandler(req =>
        {
            accessCalled = true;
            capturedAccessMethod = req.Method.Method;
            capturedAccessPath = req.RequestUri!.PathAndQuery;
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });

        var usersClient = new HttpClient(usersHandler) { BaseAddress = new Uri("http://users") };
        var accessClient = new HttpClient(accessHandler) { BaseAddress = new Uri("http://access") };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("users")).Returns(usersClient);
        factoryMock.Setup(f => f.CreateClient("access")).Returns(accessClient);

        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Access:DefaultUserRoleId"] = roleId.ToString()
            }!)
            .Build();

        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        await AuthProxyEndpoints.RegisterProxy(ctx, default(RegisterRequest)!, factoryMock.Object, cfg, CancellationToken.None);

        accessCalled.Should().BeTrue();
        capturedAccessMethod.Should().Be("POST");
        capturedAccessPath.Should().Be($"/access/users/{userId}/link-role/{roleId}");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be(regJson);
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task RegisterProxy_OnSuccessMalformedJson_SkipsAccessAndReturns201()
    {
        var regJson = """{"email":"no-id@example.com"}""";
        bool accessCalled = false;

        var usersHandler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(regJson, Encoding.UTF8, "application/json")
        });

        var accessHandler = new StubHandler(_ =>
        {
            accessCalled = true;
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });

        var usersClient = new HttpClient(usersHandler) { BaseAddress = new Uri("http://users") };
        var accessClient = new HttpClient(accessHandler) { BaseAddress = new Uri("http://access") };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("users")).Returns(usersClient);
        factoryMock.Setup(f => f.CreateClient("access")).Returns(accessClient);

        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Access:DefaultUserRoleId"] = Guid.NewGuid().ToString()
            }!)
            .Build();

        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        await AuthProxyEndpoints.RegisterProxy(ctx, default(RegisterRequest)!, factoryMock.Object, cfg, CancellationToken.None);

        accessCalled.Should().BeFalse();
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be(regJson);
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task RefreshProxy_UsesCookie_AndCopiesResponse()
    {
        string? capturedMethod = null;
        string? capturedPath = null;
        string[]? capturedCookies = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath = req.RequestUri!.PathAndQuery;
            capturedCookies = req.Headers.TryGetValues("Cookie", out var c) ? c.ToArray() : Array.Empty<string>();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"refreshed":true}""", Encoding.UTF8, "application/json")
            };
        });

        var authClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://auth")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("auth")).Returns(authClient);

        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Cookie"] = "auth=xyz";
        ctx.Response.Body = new MemoryStream();

        await AuthProxyEndpoints.RefreshProxy(ctx, factoryMock.Object, CancellationToken.None);

        capturedMethod.Should().Be("POST");
        capturedPath.Should().Be("/auth/refresh");
        capturedCookies.Should().NotBeNull();
        string.Join(";", capturedCookies!).Should().Contain("auth=xyz");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be("""{"refreshed":true}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task LogoutProxy_UsesCookie_AndCopiesResponse()
    {
        string? capturedMethod = null;
        string? capturedPath = null;
        string[]? capturedCookies = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath = req.RequestUri!.PathAndQuery;
            capturedCookies = req.Headers.TryGetValues("Cookie", out var c) ? c.ToArray() : Array.Empty<string>();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"loggedOut":true}""", Encoding.UTF8, "application/json")
            };
        });

        var authClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://auth")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("auth")).Returns(authClient);

        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Cookie"] = "auth=xyz";
        ctx.Response.Body = new MemoryStream();

        await AuthProxyEndpoints.LogoutProxy(ctx, factoryMock.Object, CancellationToken.None);

        capturedMethod.Should().Be("POST");
        capturedPath.Should().Be("/auth/logout");
        string.Join(";", capturedCookies!).Should().Contain("auth=xyz");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be("""{"loggedOut":true}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task MeProxy_UsesAuthorization_WhenPresent_AndSetsNoCacheHeaders()
    {
        string? capturedMethod = null;
        string? capturedPath = null;
        string? capturedAuth = null;
        bool hasCookieHeader = false;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath = req.RequestUri!.PathAndQuery;
            capturedAuth = req.Headers.Authorization?.ToString();
            hasCookieHeader = req.Headers.TryGetValues("Cookie", out _);

            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Headers.Add("X-Backend", "auth");
            resp.Content = new StringContent("""{"user":"me"}""", Encoding.UTF8, "application/json");
            resp.Headers.TransferEncodingChunked = true;
            return resp;
        });

        var authClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://auth")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("auth")).Returns(authClient);

        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Authorization"] = "Bearer test-token";
        ctx.Request.Headers["Cookie"] = "auth=xyz";
        ctx.Response.Body = new MemoryStream();

        await AuthProxyEndpoints.MeProxy(ctx, factoryMock.Object, CancellationToken.None);

        capturedMethod.Should().Be("GET");
        capturedPath.Should().Be("/auth/me");
        capturedAuth.Should().Be("Bearer test-token");
        hasCookieHeader.Should().BeFalse();

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Headers["X-Backend"].Should().Contain("auth");
        ctx.Response.Headers["Cache-Control"].Should().Contain("no-store, private");
        ctx.Response.Headers["Pragma"].Should().Contain("no-cache");
        ctx.Response.Headers["Vary"].Should().Contain("Cookie");
        ctx.Response.Headers.ContainsKey("transfer-encoding").Should().BeFalse();

        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be("""{"user":"me"}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task MeProxy_FallsBackToCookie_WhenNoAuthorization()
    {
        string[]? capturedCookies = null;
        string? capturedAuth = null;

        var handler = new StubHandler(req =>
        {
            capturedAuth = req.Headers.Authorization?.ToString();
            capturedCookies = req.Headers.TryGetValues("Cookie", out var c) ? c.ToArray() : Array.Empty<string>();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"ok":true}""", Encoding.UTF8, "application/json")
            };
        });

        var authClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://auth")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("auth")).Returns(authClient);

        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Cookie"] = "auth=xyz";
        ctx.Response.Body = new MemoryStream();

        await AuthProxyEndpoints.MeProxy(ctx, factoryMock.Object, CancellationToken.None);

        capturedAuth.Should().BeNull();
        string.Join(";", capturedCookies!).Should().Contain("auth=xyz");
    }

    [Fact]
    public void CopyCookie_CopiesCookieHeader()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Cookie"] = "sid=123; theme=dark";
        using var msg = new HttpRequestMessage(HttpMethod.Get, "/auth/me");

        AuthProxyEndpoints.CopyCookie(ctx, msg);

        msg.Headers.TryGetValues("Cookie", out var cookies).Should().BeTrue();
        string.Join(";", cookies!).Should().Contain("sid=123");
    }

    [Fact]
    public void CopyAuthOrCookie_UsesAuthorization_WhenPresent()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Authorization"] = "Bearer abc";
        ctx.Request.Headers["Cookie"] = "sid=123";

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/auth/me");

        AuthProxyEndpoints.CopyAuthOrCookie(ctx, msg);

        msg.Headers.Authorization!.ToString().Should().Be("Bearer abc");
        msg.Headers.TryGetValues("Cookie", out _).Should().BeFalse();
    }

    [Fact]
    public void CopyAuthOrCookie_FallsBackToCookie_WhenNoAuthorization()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Cookie"] = "sid=123; auth=xyz";

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/auth/me");

        AuthProxyEndpoints.CopyAuthOrCookie(ctx, msg);

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

        await AuthProxyEndpoints.ProxyCopyResponse(ctx, resp, CancellationToken.None);

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        ctx.Response.Headers["X-Test"].Should().Contain("123");
        ctx.Response.Headers["X-Content"].Should().Contain("abc");
        ctx.Response.Headers.ContainsKey("transfer-encoding").Should().BeFalse();

        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be("hello");
        ctx.Response.ContentType.Should().StartWith("text/plain");
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) => _handler = handler;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_handler(request));
    }
}
