using Messages.Domain.Reporitory;
using Messages.Infrastructure.Data;
using Messages.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Messages.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMessagesInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("MessagesDb")
                 ??
                 "Server=localhost,1433;Database=MessagesDB;User Id=sa;Password=zaq1!xsw2@;TrustServerCertificate=true;";
        services.AddDbContext<MessagesDbContext>(opt => opt.UseSqlServer(cs));
        services.AddScoped<IMessagesRepository , MessagesRepository>();

        return services;
    }
}