using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Data;

namespace User.Tests;

public static class UsersDbContextFactory
{
    public static UsersDbContext Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new UsersDbContext(options);
    }
}