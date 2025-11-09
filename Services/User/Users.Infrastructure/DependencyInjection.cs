using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Users.Core.Repositories;
using Users.Infrastructure.Configurations;
using Users.Infrastructure.Data;

namespace Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("UsersDb")
                 ?? "Server=localhost,1433;Database=MN.Users;User Id=sa;Password=zaq1!xsw2@;TrustServerCertificate=true;";
        services.AddDbContext<UsersDbContext>(o => o.UseSqlServer(cs));
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}