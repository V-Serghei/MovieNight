using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MovieNight.Gateway.DTO.Movie;
using MovieNight.Gateway.Endpoints;
using MovieNight.Gateway.Enums;
using Xunit;

namespace Gateway.Tests.Endpoints;

public class MoviesProxyEndpointsTests
{
    [Fact]
    public async Task ListProxy_ForwardsQuery_AndAuth()
    {
        // Arrange
        string? capturedMethod = null;
        string? capturedPath = null;
        string? capturedAuth = null;

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

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://movies") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("movies")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path   = "/movies";
        ctx.Request.QueryString = new QueryString("?page=2&take=10");
        ctx.Request.Headers["Authorization"] = "Bearer token";
        ctx.Response.Body = new MemoryStream();

        // Act
        await MoviesProxyEndpoints.ListProxy(ctx, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("GET");
        capturedPath.Should().Be("/movies?page=2&take=10");
        capturedAuth.Should().Be("Bearer token");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"items":[]}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task GetByIdProxy_ForwardsId_AndAuth()
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
                Content = new StringContent("""{"id":"movie-1"}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://movies") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("movies")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path   = "/movies";
        ctx.Request.Headers["Authorization"] = "Bearer token";
        ctx.Response.Body = new MemoryStream();

        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        await MoviesProxyEndpoints.GetByIdProxy(ctx, id, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("GET");
        capturedPath.Should().Be($"/movies/{id}");
        capturedAuth.Should().Be("Bearer token");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"id":"movie-1"}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task SearchProxy_BuildsEscapedUrl_AndAuth()
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
                Content = new StringContent("""{"items":[]}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://movies") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("movies")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path   = "/movies/search";
        ctx.Request.Headers["Authorization"] = "Bearer token";
        ctx.Response.Body = new MemoryStream();

        var title    = "Star Wars";
        var year     = 1977;
        var director = "George Lucas";

        // Act
        await MoviesProxyEndpoints.SearchProxy(ctx, title, year, director, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("GET");
        capturedPath.Should()
            .Be("/movies/search?title=Star%20Wars&year=1977&director=George%20Lucas");
        capturedAuth.Should().Be("Bearer token");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"items":[]}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task CreateProxy_SerializesBody_AndUsesPost()
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
                Content = new StringContent("""{"id":"movie-1"}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://movies") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("movies")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "POST";
        ctx.Request.Path   = "/movies";
        ctx.Request.Headers["Authorization"] = "Bearer token";
        ctx.Response.Body = new MemoryStream();

        var body = new CreateMovieRequest(
            Title: "Test Movie",
            Category: MovieCategory.Film,
            PosterImage: "/images/poster.jpg",
            Quote: "Test quote",
            Description: "Some description",
            ProductionYear: 2024,
            Country: "USA",
            Director: "Some Director",
            Duration: "120 min",
            Certificate: "PG-13",
            ProductionCompany: "Test Studio",
            Budget: "100M",
            GrossWorldwide: "200M",
            Language: "English",
            Genre: new List<string> { "Action", "Drama" },
            Cards: new List<MovieCardRequest>(),
            Facts: new List<MovieFactRequest>()
        );

        // Act
        await MoviesProxyEndpoints.CreateProxy(ctx, body, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("POST");
        capturedPath.Should().Be("/movies");
        capturedAuth.Should().Be("Bearer token");
        capturedContentType.Should().StartWith("application/json");
        capturedBody.Should().NotBeNullOrEmpty();

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"id":"movie-1"}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task SeedProxy_SerializesBody_AndPostsToSeed()
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

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"seeded":true}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://movies") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("movies")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "POST";
        ctx.Request.Path   = "/movies/seed";
        ctx.Response.Body  = new MemoryStream();

        var body = new SeedMoviesRequest("seed-movies.json");

        // Act
        await MoviesProxyEndpoints.SeedProxy(ctx, body, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("POST");
        capturedPath.Should().Be("/movies/seed");
        capturedBody.Should().NotBeNullOrEmpty();

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        ctx.Response.Body.Position = 0;
        var respBody = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        respBody.Should().Be("""{"seeded":true}""");
        ctx.Response.ContentType.Should().StartWith("application/json");
    }

    [Theory]
    [InlineData("Film",   nameof(MoviesProxyEndpoints.FilmsAliasProxy))]
    [InlineData("Cartoon",nameof(MoviesProxyEndpoints.CartonAliasProxy))]
    [InlineData("Anime",  nameof(MoviesProxyEndpoints.AnimeAliasProxy))]
    [InlineData("Serial", nameof(MoviesProxyEndpoints.SerialAliasProxy))]
    public async Task CategoryAliasProxies_CallCorrectCategory(string category, string _)
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
                Content = new StringContent("""{"items":[]}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://movies") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("movies")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Response.Body  = new MemoryStream();

        // Act
        switch (category)
        {
            case "Film":
                await MoviesProxyEndpoints.FilmsAliasProxy(ctx, factoryMock.Object, CancellationToken.None);
                break;
            case "Cartoon":
                await MoviesProxyEndpoints.CartonAliasProxy(ctx, factoryMock.Object, CancellationToken.None);
                break;
            case "Anime":
                await MoviesProxyEndpoints.AnimeAliasProxy(ctx, factoryMock.Object, CancellationToken.None);
                break;
            case "Serial":
                await MoviesProxyEndpoints.SerialAliasProxy(ctx, factoryMock.Object, CancellationToken.None);
                break;
        }

        // Assert
        capturedMethod.Should().Be("GET");
        capturedPath.Should().Be($"/movies/by-category/{category}");

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task ProxyAny_ForwardsMethod_Path_Query_Body_AndHeaders()
    {
        // Arrange
        string? capturedMethod = null;
        string? capturedPath   = null;
        string? capturedBody   = null;
        string[]? capturedCustomHeader = null;
        string? capturedAuth   = null;

        var handler = new StubHandler(req =>
        {
            capturedMethod = req.Method.Method;
            capturedPath   = req.RequestUri!.PathAndQuery;
            capturedBody   = req.Content is null
                ? null
                : req.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            capturedCustomHeader = req.Headers.TryGetValues("X-Custom", out var vals)
                ? vals.ToArray()
                : Array.Empty<string>();

            capturedAuth = req.Headers.Authorization?.ToString();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"ok":true}""", Encoding.UTF8, "application/json")
            };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://movies") };
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("movies")).Returns(client);

        var ctx = new DefaultHttpContext();
        ctx.Request.Method      = "PATCH";
        ctx.Request.Path        = "/movies/ignored-path";
        ctx.Request.QueryString = new QueryString("?page=2");
        ctx.Request.ContentType = "application/json";
        var bodyJson = """{"title":"Updated"}""";
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(bodyJson));
        ms.Position = 0;
        ctx.Request.Body         = ms;
        ctx.Request.ContentLength = ms.Length;
        ctx.Request.Headers["X-Custom"]     = "123";
        ctx.Request.Headers["Authorization"] = "Bearer xyz";

        ctx.Response.Body = new MemoryStream();

        var pathParam = "special/action";

        // Act
        await MoviesProxyEndpoints.ProxyAny(pathParam, ctx, factoryMock.Object, CancellationToken.None);

        // Assert
        capturedMethod.Should().Be("PATCH");
        capturedPath.Should().Be("/movies/special/action?page=2");
        capturedBody.Should().Be(bodyJson);
        capturedCustomHeader.Should().Contain("123");
        capturedAuth.Should().Be("Bearer xyz");

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
        using var msg = new HttpRequestMessage(HttpMethod.Get, "/movies");

        // Act
        MoviesProxyEndpoints.CopyCookie(ctx, msg);

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
        ctx.Request.Headers["Cookie"] = "sid=123";
        using var msg = new HttpRequestMessage(HttpMethod.Get, "/movies");

        // Act
        MoviesProxyEndpoints.CopyAuthOrCookie(ctx, msg);

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
        using var msg = new HttpRequestMessage(HttpMethod.Get, "/movies");

        // Act
        MoviesProxyEndpoints.CopyAuthOrCookie(ctx, msg);

        // Assert
        msg.Headers.TryGetValues("Cookie", out var cookies).Should().BeTrue();
        string.Join(";", cookies!).Should().Contain("auth=xyz");
    }

    [Fact]
    public async Task ProxyCopyResponse_CopiesStatusHeadersAndBody()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        var resp = new HttpResponseMessage(HttpStatusCode.Accepted);
        resp.Headers.Add("X-Test", "123");
        resp.Content = new StringContent("hello", Encoding.UTF8, "text/plain");
        resp.Content.Headers.Add("X-Content", "abc");

        // Act
        await MoviesProxyEndpoints.ProxyCopyResponse(ctx, resp, CancellationToken.None);

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
