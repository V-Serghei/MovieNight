using Media.Core.Repositories;
using Media.Infrastructure.Data;
using Media.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Media.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMediaInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("MediaDb")
                 ?? "Server=localhost,1433;Database=MN.Media;User Id=sa;Password=zaq1!xsw2@;TrustServerCertificate=true;";
        services.AddDbContext<MediaDbContext>(o => o.UseSqlServer(cs));
        services.AddScoped<IMediaRepository, MediaRepository>();
        
        return services;
    }
}
