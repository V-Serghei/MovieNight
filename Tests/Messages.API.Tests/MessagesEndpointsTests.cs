using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Messages.API.DTO;
using Messages.API.Endpoints;
using Messages.Domain.Entities;
using Messages.Domain.Reporitory;
using Xunit;

namespace Messages.API.Tests;

public class MessagesEndpointsTests
{
    private readonly Mock<IMessagesRepository> _repoMock = new(MockBehavior.Strict);

    private TestServer CreateServer()
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddSingleton(_repoMock.Object);
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    // Подключаем тот же самый extension, что и в рабочем сервисе
                    endpoints.MapMessagesEndpoints();
                });
            });

        return new TestServer(builder);
    }

    private HttpClient CreateClient(out TestServer server)
    {
        server = CreateServer();
        return server.CreateClient();
    }

    [Fact]
    public async Task GetById_Should_ReturnNotFound_WhenMessageDoesNotExist()
    {
        // Arrange
        using var server = CreateServer();
        using var client = server.CreateClient();

        var id = Guid.NewGuid();

        _repoMock
            .Setup(r => r.FindByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Messages.Domain.Entities.Messages?)null);

        // Act
        var response = await client.GetAsync($"/messages/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repoMock.Verify(r => r.FindByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.VerifyNoOtherCalls();
    }
}
