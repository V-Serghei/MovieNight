using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Access.Core.Entities;
using Access.Infrastructure.Data;
using Access.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Access.Tests;

public class AccessRepositoryTests
{
    private static AccessDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AccessDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new AccessDbContext(options);
    }

    [Fact]
    public async Task GetUserRolesAsync_Should_ReturnRolesLinkedToUser()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var userId = Guid.NewGuid();

        var roleA = new Role { Name = "admin" };
        var roleB = new Role { Name = "editor" };
        var roleOther = new Role { Name = "random" };

        db.Roles.AddRange(roleA, roleB, roleOther);
        db.UserRoles.AddRange(
            new UserRole { UserId = userId, RoleId = roleA.Id },
            new UserRole { UserId = userId, RoleId = roleB.Id },
            new UserRole { UserId = Guid.NewGuid(), RoleId = roleOther.Id }
        );

        await db.SaveChangesAsync();

        var repo = new AccessRepository(db);

        var roles = await repo.GetUserRolesAsync(userId, CancellationToken.None);

        roles.Should().HaveCount(2);
        roles.Select(r => r.Name).Should().BeEquivalentTo("admin", "editor");
    }

    [Fact]
    public async Task GetUserRolesAsync_Should_ReturnEmpty_WhenNoRoles()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var repo = new AccessRepository(db);

        var roles = await repo.GetUserRolesAsync(Guid.NewGuid(), CancellationToken.None);

        roles.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPoliciesForRolesAsync_Should_ReturnPoliciesForGivenRoles()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var roleA = new Role { Name = "rA" };
        var roleB = new Role { Name = "rB" };

        var pol1 = new Policy { Resource = "movies", Method = "read" };
        var pol2 = new Policy { Resource = "movies", Method = "write" };
        var pol3 = new Policy { Resource = "users", Method = "read" };

        db.Roles.AddRange(roleA, roleB);
        db.Policies.AddRange(pol1, pol2, pol3);

        db.RolePolicies.AddRange(
            new RolePolicy { RoleId = roleA.Id, PolicyId = pol1.Id },
            new RolePolicy { RoleId = roleA.Id, PolicyId = pol2.Id },
            new RolePolicy { RoleId = roleB.Id, PolicyId = pol3.Id }
        );

        await db.SaveChangesAsync();

        var repo = new AccessRepository(db);

        var result = await repo.GetPoliciesForRolesAsync(
            new[] { roleA.Id, roleB.Id },
            CancellationToken.None
        );

        result.Should().HaveCount(3);
        result.Select(p => p.Resource).Should().BeEquivalentTo("movies", "movies", "users");
    }

    [Fact]
    public async Task GetPoliciesForRolesAsync_Should_ReturnEmpty_WhenRolesNotLinked()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var repo = new AccessRepository(db);

        var result = await repo.GetPoliciesForRolesAsync(Array.Empty<Guid>(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPoliciesForUserAsync_Should_ReturnPoliciesLinkedToUser()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var userId = Guid.NewGuid();

        var pol1 = new Policy { Resource = "r1" };
        var pol2 = new Policy { Resource = "r2" };
        var polOther = new Policy { Resource = "other" };

        db.Policies.AddRange(pol1, pol2, polOther);

        db.UserPolicies.AddRange(
            new UserPolicy { UserId = userId, PolicyId = pol1.Id },
            new UserPolicy { UserId = userId, PolicyId = pol2.Id },
            new UserPolicy { UserId = Guid.NewGuid(), PolicyId = polOther.Id }
        );

        await db.SaveChangesAsync();

        var repo = new AccessRepository(db);

        var result = await repo.GetPoliciesForUserAsync(userId, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(p => p.Resource).Should().BeEquivalentTo("r1", "r2");
    }

    [Fact]
    public async Task AddRoleAsync_Should_AddRole_AndNotSaveImmediately()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var repo = new AccessRepository(db);

        var role = new Role { Name = "test-role" };

        await repo.AddRoleAsync(role, CancellationToken.None);

        db.Roles.Should().BeEmpty("SaveChangesAsync was not called");

        await repo.SaveChangesAsync(CancellationToken.None);

        db.Roles.Should().ContainSingle(
            r => r.Name == "test-role"
        );
    }

    [Fact]
    public async Task AddPolicyAsync_Should_AddPolicy_AndNotSaveImmediately()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var repo = new AccessRepository(db);

        var policy = new Policy { Resource = "res1", Method = "read" };

        await repo.AddPolicyAsync(policy, CancellationToken.None);

        db.Policies.Should().BeEmpty();
        await repo.SaveChangesAsync(CancellationToken.None);

        db.Policies.Should().ContainSingle(p => p.Resource == "res1");
    }

    [Fact]
    public async Task LinkRolePolicyAsync_Should_CreateLink()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var role = new Role { Name = "role" };
        var policy = new Policy { Resource = "resX" };

        db.Roles.Add(role);
        db.Policies.Add(policy);
        await db.SaveChangesAsync();

        var repo = new AccessRepository(db);

        await repo.LinkRolePolicyAsync(role.Id, policy.Id, CancellationToken.None);
        await repo.SaveChangesAsync(CancellationToken.None);

        db.RolePolicies.Should().ContainSingle(
            rp => rp.RoleId == role.Id && rp.PolicyId == policy.Id
        );
    }

    [Fact]
    public async Task LinkUserRoleAsync_Should_CreateLink()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var userId = Guid.NewGuid();
        var role = new Role { Name = "role1" };

        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var repo = new AccessRepository(db);

        await repo.LinkUserRoleAsync(userId, role.Id, CancellationToken.None);
        await repo.SaveChangesAsync(CancellationToken.None);

        db.UserRoles.Should().ContainSingle(
            ur => ur.UserId == userId && ur.RoleId == role.Id
        );
    }
}
