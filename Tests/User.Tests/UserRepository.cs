using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using User.Tests;
using Users.Core.Entities;
using Users.Infrastructure.Configurations;
using Users.Infrastructure.Data;
using Xunit;

namespace Users.Tests;

public class UserRepositoryTests
{
    private static UsersDbContext CreateContext()
        => UsersDbContextFactory.Create(Guid.NewGuid().ToString());

    [Fact]
    public async Task ExistsByEmailAsync_Should_ReturnTrue_WhenUserExists()
    {
        await using var db = CreateContext();

        var user = new Core.Entities.User
        {
            Email = "test@example.com",
            EmailNormalized = "TEST@EXAMPLE.COM",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            DisplayName = "Test"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var repo = new UserRepository(db);

        var exists = await repo.ExistsByEmailAsync("TEST@EXAMPLE.COM");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_Should_ReturnFalse_WhenUserDoesNotExist()
    {
        await using var db = CreateContext();
        var repo = new UserRepository(db);

        var exists = await repo.ExistsByEmailAsync("UNKNOWN@EXAMPLE.COM");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task FindByEmailAsync_Should_ReturnUser_WhenExists()
    {
        await using var db = CreateContext();

        var user = new Core.Entities.User
        {
            Email = "test@example.com",
            EmailNormalized = "TEST@EXAMPLE.COM",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            DisplayName = "Test"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var repo = new UserRepository(db);

        var found = await repo.FindByEmailAsync("TEST@EXAMPLE.COM");

        found.Should().NotBeNull();
        found!.Id.Should().Be(user.Id);
        found.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task FindByEmailAsync_Should_ReturnNull_WhenNotExists()
    {
        await using var db = CreateContext();
        var repo = new UserRepository(db);

        var found = await repo.FindByEmailAsync("UNKNOWN@EXAMPLE.COM");

        found.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnUser_WhenExists()
    {
        await using var db = CreateContext();

        var user = new Core.Entities.User
        {
            Email = "id@example.com",
            EmailNormalized = "ID@EXAMPLE.COM",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            DisplayName = "ById User"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var repo = new UserRepository(db);

        var found = await repo.GetByIdAsync(user.Id);

        found.Should().NotBeNull();
        found!.Email.Should().Be("id@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        await using var db = CreateContext();
        var repo = new UserRepository(db);

        var found = await repo.GetByIdAsync(Guid.NewGuid());

        found.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnAllUsers()
    {
        await using var db = CreateContext();

        db.Users.AddRange(
            new Core.Entities.User
            {
                Email = "u1@example.com",
                EmailNormalized = "U1@EXAMPLE.COM",
                PasswordHash = "hash1",
                PasswordSalt = "salt1",
                DisplayName = "User1"
            },
            new Core.Entities.User
            {
                Email = "u2@example.com",
                EmailNormalized = "U2@EXAMPLE.COM",
                PasswordHash = "hash2",
                PasswordSalt = "salt2",
                DisplayName = "User2"
            }
        );

        await db.SaveChangesAsync();

        var repo = new UserRepository(db);

        var list = await repo.GetAllAsync();

        list.Should().NotBeNull();
        list!.Should().HaveCount(2);
        list.Select(u => u.Email).Should().BeEquivalentTo(new[]
        {
            "u1@example.com",
            "u2@example.com"
        });
    }

    [Fact]
    public async Task AddAsync_And_SaveChangesAsync_Should_PersistUser()
    {
        await using var db = CreateContext();
        var repo = new UserRepository(db);

        var user = new Core.Entities.User
        {
            Email = "new@example.com",
            EmailNormalized = "NEW@EXAMPLE.COM",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            DisplayName = "New User"
        };

        await repo.AddAsync(user);
        await repo.SaveChangesAsync();

        var stored = await db.Users.SingleOrDefaultAsync(u => u.Id == user.Id);
        stored.Should().NotBeNull();
        stored!.Email.Should().Be("new@example.com");
        stored.EmailNormalized.Should().Be("NEW@EXAMPLE.COM");
    }
}
