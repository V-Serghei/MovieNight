using Access.Core.Entities;
using Access.Core.Enums;
using Access.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Access.Infrastructure.Seed;

public static class AccessSeeder
{
    public static async Task SeedAsync(AccessDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (await db.Roles.AnyAsync()) return;

        var admin = new Role { Name = "admin" };
        var user  = new Role { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "user" };
        var moderator = new Role { Name = "moderator" };

        var p1 = new Policy { Resource = "/movies/**", Method = "GET", Effect = Effect.Allow, Description = "Read any movie" };
        var p2 = new Policy { Resource = "/movies/*", Method = "POST", Effect = Effect.Deny, Description = "Users cannot POST movies" };
        var p3 = new Policy { Resource = "/admin/**", Effect = Effect.Allow, Description = "Admin everything" };

        await db.Roles.AddRangeAsync(admin, user, moderator);
        await db.Policies.AddRangeAsync(p1, p2, p3);
        await db.SaveChangesAsync();

        await db.RolePolicies.AddRangeAsync(
            new RolePolicy { RoleId = user.Id, PolicyId = p1.Id },
            new RolePolicy { RoleId = user.Id, PolicyId = p2.Id },
            new RolePolicy { RoleId = admin.Id, PolicyId = p3.Id }
        );
        await db.SaveChangesAsync();
    }
}
