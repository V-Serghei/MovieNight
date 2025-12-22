using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MovieRatings.Infrastructure.Data;

namespace MovieRatings.Tests;

internal static class RatingsDbContextFactory
{
    public static RatingsDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<RatingsDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .ConfigureWarnings(w =>
            {
                w.Ignore(InMemoryEventId.TransactionIgnoredWarning);
            })
            .Options;

        return new RatingsDbContext(options);
    }
}