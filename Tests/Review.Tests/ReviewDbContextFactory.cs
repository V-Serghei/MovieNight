using Microsoft.EntityFrameworkCore;
using Review.Infrastructure.Data;

namespace Review.Tests;

internal static class ReviewDbContextFactory
{
    public static ReviewDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ReviewDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ReviewDbContext(options);
    }
}