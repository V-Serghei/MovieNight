using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MoviePlayer.Infrastructure.Data.Migrations;

public class MoviePlayerDbContextFactory : IDesignTimeDbContextFactory<MoviePlayerDbContext>
{
    public MoviePlayerDbContext CreateDbContext(string[] args)
    {
        var cfg = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = cfg.GetConnectionString("MoviePlayerDb")
                 ?? "Server=localhost,1433;Database=MNMoviePlayerDB;User Id=sa;Password=HardModNice373;TrustServerCertificate=true;";

        var b = new DbContextOptionsBuilder<MoviePlayerDbContext>()
            .UseSqlServer(cs);

        return new MoviePlayerDbContext(b.Options);
    }
}