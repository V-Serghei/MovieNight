using System;
using Auth.Domain.Repository.Tokens;
using Auth.Infrastructure.Clients;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTokensInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("TokensDb")
                 ?? "Server=localhost,1433;Database=MN.Tokens;User Id=sa;Password=zaq1!xsw2@;TrustServerCertificate=true;";

        services.AddDbContext<AuthDbContext>(o => o.UseSqlServer(cs));

        services.AddScoped<IRefreshTokensRepository, RefreshTokenRepository>();

        services.AddHttpClient<UsersClient>(c =>
        {
            var baseUrl = cfg["Gateway:BaseUrl"] ?? "http://localhost:7000";
            c.BaseAddress = new Uri(baseUrl);
            var secret = cfg["Gateway:InternalSecret"] ?? "dev-internal-secret";
            c.DefaultRequestHeaders.Add("X-Internal-Secret", secret);
        });

        return services;
    }
}