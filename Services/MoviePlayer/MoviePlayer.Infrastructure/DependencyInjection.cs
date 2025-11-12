using MoviePlayer.Domain.Repository.Movie;
using MoviePlayer.Infrastructure.Data;
using MoviePlayer.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoviePlayer.Infrastructure.Data.Migrations;

namespace MoviePlayer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMoviePlayerInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("MoviePlayerDb")
                 ??
                 "Server=localhost,1433;Database=MoviePlayerDB;User Id=sa;Password=zaq1!xsw2@;TrustServerCertificate=true;";
        services.AddDbContext<MoviePlayerDbContext>(opt => opt.UseSqlServer(cs));
        services.AddScoped<IMovieRepository, MovieRepository>();

        return services;
    }
}