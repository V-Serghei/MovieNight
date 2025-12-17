using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Auth.Infrastructure.Clients;
using FluentAssertions;
using Xunit;

namespace Auth.Tests;

public class UsersClientTests
{
    private static HttpClient CreateClient(HttpStatusCode status, object? responseBody)
    {
        var handler = new StubHandler((req, ct) =>
        {
            var resp = new HttpResponseMessage(status);
            if (responseBody != null)
                resp.Content = JsonContent.Create(responseBody);

            return Task.FromResult(resp);
        });

        return new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
    }

    [Fact]
    public async Task VerifyAsync_Should_ReturnUser_WhenStatusIs200()
    {
        var response = new UsersClient.VerifyResponse(
            ok: true,
            new UsersClient.UserDto(Guid.NewGuid(), "a@b.com", "Alex", true)
        );

        var client = new UsersClient(CreateClient(HttpStatusCode.OK, response));

        var result = await client.VerifyAsync("a@b.com", "123", CancellationToken.None);

        result.ok.Should().BeTrue();
        result.user.Should().NotBeNull();
        result.user!.Email.Should().Be("a@b.com");
    }

    [Fact]
    public async Task VerifyAsync_Should_Throw_WhenStatusIsNot200()
    {
        var http = CreateClient(HttpStatusCode.BadRequest, null);
        var client = new UsersClient(http);

        Func<Task> act = () => client.VerifyAsync("x", "y", CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetUser_Should_ReturnUser_WhenStatusIs200()
    {
        var dto = new UsersClient.UserDto(Guid.NewGuid(), "u@t.com", "Test", true);
        var http = CreateClient(HttpStatusCode.OK, dto);
        var client = new UsersClient(http);

        var result = await client.GetUser(dto.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Email.Should().Be("u@t.com");
    }

    [Fact]
    public async Task GetUser_Should_ReturnNull_WhenStatusIs404()
    {
        var http = CreateClient(HttpStatusCode.NotFound, null);
        var client = new UsersClient(http);

        var result = await client.GetUser(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _handler(request, cancellationToken);
    }
}
