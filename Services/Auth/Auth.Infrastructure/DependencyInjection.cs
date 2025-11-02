using Auth.Domain.Repository.User;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("AuthDb")
                 ??
                 "Server=localhost,1433;Database=MNAuthDB;User Id=sa;Password=zaq1!xsw2@;TrustServerCertificate=true;";
        services.AddDbContext<AuthDbContext>(opt => opt.UseSqlServer(cs));
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}