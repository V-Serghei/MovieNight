using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieRatings.Domain.Repository;
using MovieRatings.Infrastructure.Data;
using MovieRatings.Infrastructure.Repositories;

namespace MovieRatings.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRatingsInfrastructure(
        this IServiceCollection services,
        IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("RatingsDb")
                 ?? "Server=localhost,1433;Database=MN.Ratings;User Id=sa;Password=zaq1!xsw2@;TrustServerCertificate=true;";

        services.AddDbContext<RatingsDbContext>(o =>
        {
            o.UseSqlServer(cs);
        });

        services.AddScoped<IRatingsRepository, RatingsRepository>();

        return services;
    }
}