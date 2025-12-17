using Microsoft.EntityFrameworkCore;
using People.Infrastructure.Data;

namespace People.Tests;

internal static class PeopleDbContextFactory
{
    public static PeopleDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<PeopleDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new PeopleDbContext(options);
    }
}