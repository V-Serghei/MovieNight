using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using People.Domain.Repository;
using People.Infrastructure.Data;
using People.Infrastructure.Repositories;

namespace People.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPeopleInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("PeopleDb")
                 ?? "Server=localhost,1433;Database=MN.People;User Id=sa;Password=zaq1!xsw2@;TrustServerCertificate=true;";

        services.AddDbContext<PeopleDbContext>(o => o.UseSqlServer(cs));

        services.AddScoped<IPeopleRepository, PeopleRepository>();

        return services;
    }
}