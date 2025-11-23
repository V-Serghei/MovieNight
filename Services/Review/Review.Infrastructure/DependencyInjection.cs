using Review.Domain.Repository.Review;
using Review.Infrastructure.Data;
using Review.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Review.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReviewInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("ReviewDb")
                 ??
                 "Server=localhost,1433;Database=ReviewDB;User Id=sa;Password=zaq1!xsw2@;TrustServerCertificate=true;";
        services.AddDbContext<ReviewDbContext>(opt => opt.UseSqlServer(cs));
        services.AddScoped<IReviewRepository, ReviewRepository>();

        return services;
    }
}