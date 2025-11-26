using Bookmark.Domain.Repository;
using Bookmark.Infrastructure.Data;
using Bookmark.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bookmark.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBookmarksInfrastructure(
        this IServiceCollection services,
        IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("BookmarksDb")
                 ?? "Server=localhost,1433;Database=MN.Bookmarks;User Id=sa;Password=zaq1!xsw2@;TrustServerCertificate=true;";

        services.AddDbContext<BookmarksDbContext>(o =>
        {
            o.UseSqlServer(cs);
        });

        services.AddScoped<IBookmarksRepository, BookmarksRepository>();

        return services;
    }
}