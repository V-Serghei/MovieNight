using Access.Core.Repositories;
using Access.Core.Services;
using Access.Infrastructure.Data;
using Access.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
namespace Access.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAccessInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("AccessDb")
                 ?? "Server=localhost,1433;Database=MN.Access;User Id=sa;Password=zaq1!xsw2@;TrustServerCertificate=true;";
        services.AddDbContext<AccessDbContext>(o => o.UseSqlServer(cs));
        services.AddScoped<IAccessRepository, AccessRepository>();
        services.AddScoped<IAccessEvaluator, AccessEvaluator>();
        return services;
    }
}
