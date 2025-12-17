using System.Collections;
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

public class RatingsProxyEndpointsTests
{
    [Fact]
    public async Task ProxyPassthrough_Forwards_Method_Path_Query_AndHeaders()
    {
        // Arrange
        string? capturedPathAndQuery = null;
        string? capturedMethod = null;
        string? capturedAuth = null;
        string[]? capturedUserIds = null;
        string[]? capturedRoles = null;

        var handler = new StubHandler(req =>
        {
            capturedPathAndQuery = req.RequestUri!.PathAndQuery;
            capturedMethod = req.Method.Method;
            capturedAuth = req.Headers.Authorization?.ToString();
            capturedUserIds = req.Headers.TryGetValues("X-UserId", out var uids)
                ? uids.ToArray()
                : Array.Empty<string>();
            capturedRoles = req.Headers.TryGetValues("X-UserRole", out var r)
                ? r.ToArray()
                : Array.Empty<string>();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"ok":true}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://ratings")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(x => x.CreateClient("ratings")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path = "/ratings/movies/11111111-1111-1111-1111-111111111111";
        ctx.Request.QueryString = new QueryString("?includeDetails=true");
        ctx.Request.Headers["Authorization"] = "Bearer test-token";

        var identity = new ClaimsIdentity(new[]
        {
            new Claim("sub", "user-123"),
            new Claim(ClaimTypes.Role, "user")
        }, "test");
        ctx.User = new ClaimsPrincipal(identity);

        ctx.Response.Body = new MemoryStream();

        // Act
        await RatingsProxyEndpoints.ProxyPassthrough(ctx, factoryMock.Object, CancellationToken.None);

        // Assert: что ушло на бэкенд
        capturedMethod.Should().Be("GET");
        capturedPathAndQuery.Should()
            .Be("/ratings/movies/11111111-1111-1111-1111-111111111111?includeDetails=true");

        capturedAuth.Should().Be("Bearer test-token");

        capturedUserIds.Should().Contain("user-123");
        capturedRoles.Should().Contain("user");

        // Assert: что вернулось клиенту
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be("""{"ok":true}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task ProxyWithBody_ForwardsBody_Path_AndUserHeaders()
    {
        // Arrange
        string? capturedPathAndQuery = null;
        string? capturedMethod = null;
        string? capturedBody = null;
        string[]? capturedUserIds = null;
        string[]? capturedRoles = null;

        var handler = new StubHandler(req =>
        {
            capturedPathAndQuery = req.RequestUri!.PathAndQuery;
            capturedMethod = req.Method.Method;
            capturedBody = req.Content is null
                ? null
                : req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            capturedUserIds = req.Headers.TryGetValues("X-UserId", out var uids)
                ? uids.ToArray()
                : Array.Empty<string>();
            capturedRoles = req.Headers.TryGetValues("X-UserRole", out var r)
                ? r.ToArray()
                : Array.Empty<string>();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"updated":true}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://ratings")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(x => x.CreateClient("ratings")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "PUT";
        ctx.Request.Path = "/ratings/movies/11111111-1111-1111-1111-111111111111";
        ctx.Request.QueryString = new QueryString("?source=client");

        var requestJson = """{"score":9}""";
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));
        ms.Position = 0;
        ctx.Request.Body = ms;
        ctx.Request.ContentLength = ms.Length;
        ctx.Request.ContentType = "application/json";

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-999"),
            new Claim(ClaimTypes.Role, "admin")
        }, "test");
        ctx.User = new ClaimsPrincipal(identity);

        ctx.Response.Body = new MemoryStream();

        // Act
        await RatingsProxyEndpoints.ProxyWithBody(ctx, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("PUT");
        capturedPathAndQuery.Should()
            .Be("/ratings/movies/11111111-1111-1111-1111-111111111111?source=client");
        capturedBody.Should().Be(requestJson);

        capturedUserIds.Should().Contain("user-999");
        capturedRoles.Should().Contain("admin");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"updated":true}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public void CopyCookie_CopiesCookieHeader()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Cookie"] = "sid=123; theme=dark";

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/ratings");

        // Act
        RatingsProxyEndpoints.CopyCookie(ctx, msg);

        // Assert
        msg.Headers.TryGetValues("Cookie", out var cookies).Should().BeTrue();
        string.Join(";", cookies!).Should().Contain("sid=123");
    }

    [Fact]
    public void CopyAuthOrCookie_UsesAuthorizationHeader_WhenPresent()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Authorization"] = "Bearer abc";

        // Куки тоже можно подложить для проверки, что они *не* используются
        ctx.Request.Cookies = new TestRequestCookieCollection(new Dictionary<string, string>
        {
            ["access_token"] = "should-not-be-used"
        });

        var identity = new ClaimsIdentity(new[]
        {
            new Claim("sub", "user-123")
        }, "test");
        ctx.User = new ClaimsPrincipal(identity);

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/ratings");

        // Act
        RatingsProxyEndpoints.CopyAuthOrCookie(ctx, msg);

        // Assert
        msg.Headers.Authorization!.ToString().Should().Be("Bearer abc");
        msg.Headers.GetValues("X-UserId").Should().Contain("user-123");
    }

    [Fact]
    public void CopyAuthOrCookie_UsesAccessTokenCookie_WhenAuthorizationMissing()
    {
        // Arrange
        var ctx = new DefaultHttpContext();

        ctx.Request.Cookies = new TestRequestCookieCollection(new Dictionary<string, string>
        {
            ["access_token"] = "cookie-token"
        });

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/ratings");

        // Act
        RatingsProxyEndpoints.CopyAuthOrCookie(ctx, msg);

        // Assert
        msg.Headers.Authorization!.ToString().Should().Be("Bearer cookie-token");
    }

    [Fact]
    public async Task ProxyCopyResponse_CopiesStatusHeadersAndBody()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        var resp = new HttpResponseMessage(HttpStatusCode.Accepted);
        resp.Headers.Add("X-Test-Header", "123");
        resp.Content = new StringContent("hello", Encoding.UTF8, "text/plain");
        resp.Content.Headers.Add("X-Content-Header", "abc");

        // Act
        await RatingsProxyEndpoints.ProxyCopyResponse(ctx, resp, CancellationToken.None);

        // Assert
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        ctx.Response.Headers["X-Test-Header"].Should().Contain("123");
        ctx.Response.Headers["X-Content-Header"].Should().Contain("abc");

        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be("hello");
        ctx.Response.ContentType.Should().StartWith("text/plain");
    }

    [Fact]
    public void AddUserHeaders_AddsIdAndRoleFromClaims()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-555"),
            new Claim(ClaimTypes.Role, "critic")
        }, "test");
        ctx.User = new ClaimsPrincipal(identity);

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/ratings");

        // Act
        RatingsProxyEndpoints.AddUserHeaders(ctx, msg);

        // Assert
        msg.Headers.GetValues("X-UserId").Single().Should().Be("user-555");
        msg.Headers.GetValues("X-UserRole").Single().Should().Be("critic");
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

    private sealed class TestRequestCookieCollection : IRequestCookieCollection
    {
        private readonly Dictionary<string, string> _cookies;

        public TestRequestCookieCollection(IDictionary<string, string>? cookies = null)
        {
            _cookies = cookies != null
                ? new Dictionary<string, string>(cookies)
                : new Dictionary<string, string>();
        }

        public string this[string key] => _cookies.TryGetValue(key, out var value) ? value : string.Empty;

        public int Count => _cookies.Count;

        public ICollection<string> Keys => _cookies.Keys;

        public bool ContainsKey(string key) => _cookies.ContainsKey(key);

        public bool TryGetValue(string key, out string value) => _cookies.TryGetValue(key, out value!);

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _cookies.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _cookies.GetEnumerator();
    }
}
