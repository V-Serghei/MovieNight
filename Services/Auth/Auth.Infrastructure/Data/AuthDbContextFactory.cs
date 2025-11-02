using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Auth.Infrastructure.Data;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var cfg = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = cfg.GetConnectionString("AuthDb")
                 ?? "Server=localhost,1433;Database=MNAuthDB;User Id=sa;Password=HardModNice373;TrustServerCertificate=true;";

        var b = new DbContextOptionsBuilder<AuthDbContext>()
            .UseSqlServer(cs);

        return new AuthDbContext(b.Options);
    }
}