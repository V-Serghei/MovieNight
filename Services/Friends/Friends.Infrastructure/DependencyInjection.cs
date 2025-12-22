using Friends.Domain.Repository;
using Friends.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Friends.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFriendsInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("FriendsDb")
                 ??
                 "Server=localhost,1433;Database=FriendsDB;User Id=sa;Password=zaq1!xsw2@;TrustServerCertificate=true;";
        services.AddDbContext<FriendsDbContext>(opt => opt.UseSqlServer(cs));
        services.AddScoped<IFriendsRepository, FriendsRepository>();

        return services;
    }
}