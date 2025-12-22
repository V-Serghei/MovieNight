using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MovieNight.Gateway.Infrastructure;

namespace Gateway.Tests;

public class GatewayFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Подменяем IAclClient на "всегда разрешено"
            services.AddSingleton<IAclClient>(_ => new FakeAclClient());

            // Можно ещё подменить аутентификацию на тестовую схему
            // и/или HttpClient-ы, если хотим "настоящий" e2e.
        });
    }

    private sealed class FakeAclClient : IAclClient
    {
        public Task<bool> IsAllowedAsync(Guid userId, string resource, string method, CancellationToken ct = default)
            => Task.FromResult(true);
    }
}
