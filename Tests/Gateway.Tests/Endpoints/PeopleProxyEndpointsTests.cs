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

public class PeopleProxyEndpointsTests
{
    [Fact]
    public async Task CreatePersonProxy_ForwardsBody_Auth_AndCopiesResponse()
    {
        // Arrange
        string? capturedMethod = null;
        string? capturedPath = null;
        string? capturedAuth = null;
        string? capturedBody = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath = req.RequestUri!.PathAndQuery;
            capturedAuth = req.Headers.Authorization?.ToString();
            capturedBody = req.Content is null
                ? null
                : req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"id":"person-1"}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://people")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("people")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "POST";
        ctx.Request.Path = "/people";
        ctx.Request.Headers["Authorization"] = "Bearer abc";
        ctx.Response.Body = new MemoryStream();

        var body = new { name = "John Doe", role = "Actor" };

        // Act
        await PeopleProxyEndpoints.CreatePersonProxy(ctx, body, factoryMock.Object, CancellationToken.None);

        // Assert: исходящий запрос
        capturedMethod.Should().Be("POST");
        capturedPath.Should().Be("/people");
        capturedAuth.Should().Be("Bearer abc");

        capturedBody.Should().NotBeNull();
        using (var doc = JsonDocument.Parse(capturedBody!))
        {
            var root = doc.RootElement;
            root.GetProperty("name").GetString().Should().Be("John Doe");
            root.GetProperty("role").GetString().Should().Be("Actor");
        }

        // Ответ клиенту
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"id":"person-1"}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task GetPersonByIdProxy_ForwardsGetById_AndCopiesResponse()
    {
        // Arrange
        string? capturedMethod = null;
        string? capturedPath = null;
        string? capturedAuth = null;
        string? capturedCookie = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath = req.RequestUri!.PathAndQuery;
            capturedAuth = req.Headers.Authorization?.ToString();
            capturedCookie = req.Headers.TryGetValues("Cookie", out var cookies)
                ? string.Join(";", cookies)
                : null;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"id":"person-1"}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://people")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("people")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path = "/people";
        ctx.Request.Headers["Authorization"] = "Bearer token";
        ctx.Request.Headers["Cookie"] = "sid=123";
        ctx.Response.Body = new MemoryStream();

        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        await PeopleProxyEndpoints.GetPersonByIdProxy(ctx, id, factoryMock.Object, CancellationToken.None);

        // Assert: исходящий запрос
        capturedMethod.Should().Be("GET");
        capturedPath.Should().Be($"/people/{id}");
        capturedAuth.Should().Be("Bearer token");
        // Cookie не должен копироваться, т.к. есть Authorization
        capturedCookie.Should().BeNull();

        // Ответ клиенту
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"id":"person-1"}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task SearchPeopleProxy_ForwardsEscapedName_AndCopiesResponse()
    {
        // Arrange
        string? capturedMethod = null;
        string? capturedPath = null;
        string? capturedAuth = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath = req.RequestUri!.PathAndQuery;
            capturedAuth = req.Headers.Authorization?.ToString();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"results":[]}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://people")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("people")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path = "/people/search";
        ctx.Request.Headers["Authorization"] = "Bearer token";
        ctx.Response.Body = new MemoryStream();

        var name = "John Doe";

        // Act
        await PeopleProxyEndpoints.SearchPeopleProxy(ctx, name, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("GET");
        capturedPath.Should().Be("/people/search?name=John%20Doe");
        capturedAuth.Should().Be("Bearer token");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"results":[]}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task CreateCreditProxy_ForwardsBody_AndAuth()
    {
        // Arrange
        string? capturedMethod = null;
        string? capturedPath = null;
        string? capturedAuth = null;
        string? capturedBody = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath = req.RequestUri!.PathAndQuery;
            capturedAuth = req.Headers.Authorization?.ToString();
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
            BaseAddress = new Uri("http://people")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("people")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "POST";
        ctx.Request.Path = "/people/credits";
        ctx.Request.Headers["Authorization"] = "Bearer token";
        ctx.Response.Body = new MemoryStream();

        var body = new { movieId = 1, personId = 2, role = "Director" };

        // Act
        await PeopleProxyEndpoints.CreateCreditProxy(ctx, body, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("POST");
        capturedPath.Should().Be("/people/credits");
        capturedAuth.Should().Be("Bearer token");

        capturedBody.Should().NotBeNull();
        using (var doc = JsonDocument.Parse(capturedBody!))
        {
            var root = doc.RootElement;
            root.GetProperty("movieId").GetInt32().Should().Be(1);
            root.GetProperty("personId").GetInt32().Should().Be(2);
            root.GetProperty("role").GetString().Should().Be("Director");
        }

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"ok":true}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task GetCreditsForMovieProxy_ForwardsMovieId_AndCopiesResponse()
    {
        // Arrange
        string? capturedMethod = null;
        string? capturedPath = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath = req.RequestUri!.PathAndQuery;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"credits":[]}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://people")
        };

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("people")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path = "/people/movies";
        ctx.Response.Body = new MemoryStream();

        var movieId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Act
        await PeopleProxyEndpoints.GetCreditsForMovieProxy(ctx, movieId, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("GET");
        capturedPath.Should().Be($"/people/movies/{movieId}/credits");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"credits":[]}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public void CopyCookie_CopiesCookieHeader()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Cookie"] = "sid=123; theme=dark";
        using var msg = new HttpRequestMessage(HttpMethod.Get, "/people");

        // Act
        PeopleProxyEndpoints.CopyCookie(ctx, msg);

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
        ctx.Request.Headers["Cookie"] = "sid=123"; // не должен использоваться
        using var msg = new HttpRequestMessage(HttpMethod.Get, "/people");

        // Act
        PeopleProxyEndpoints.CopyAuthOrCookie(ctx, msg);

        // Assert
        msg.Headers.Authorization!.ToString().Should().Be("Bearer abc");
        msg.Headers.TryGetValues("Cookie", out _).Should().BeFalse();
    }

    [Fact]
    public void CopyAuthOrCookie_FallsBackToCookie_WhenAuthorizationMissing()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers["Cookie"] = "sid=123; auth=xyz";
        using var msg = new HttpRequestMessage(HttpMethod.Get, "/people");

        // Act
        PeopleProxyEndpoints.CopyAuthOrCookie(ctx, msg);

        // Assert
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
        await PeopleProxyEndpoints.ProxyCopyResponse(ctx, resp, CancellationToken.None);

        // Assert
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        ctx.Response.Headers["X-Test"].Should().Contain("123");
        ctx.Response.Headers["X-Content"].Should().Contain("abc");
        ctx.Response.ContentType.Should().StartWith("text/plain");

        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be("hello");
    }

    // Вспомогательный HttpMessageHandler для перехвата запросов
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
