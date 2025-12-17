using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Auth.Tests;

public class RefreshTokenRepositoryTests
{
    private static AuthDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new AuthDbContext(options);
    }

    [Fact]
    public async Task FindByHashAsync_Should_ReturnToken_WhenExists()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var tokenHash = "hash-123";

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = tokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
            Revoked = false
        });

        await db.SaveChangesAsync();

        var repo = new RefreshTokenRepository(db);

        var result = await repo.FindByHashAsync(tokenHash);

        result.Should().NotBeNull();
        result!.Token.Should().Be(tokenHash);
    }

    [Fact]
    public async Task FindByHashAsync_Should_ReturnNull_WhenNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var repo = new RefreshTokenRepository(db);

        var result = await repo.FindByHashAsync("unknown");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_And_SaveChangesAsync_Should_PersistToken()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var repo = new RefreshTokenRepository(db);

        var token = new RefreshToken
        {
            UserId = Guid.NewGuid(),
            Token = "tok-xyz",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(2)
        };

        await repo.AddAsync(token, CancellationToken.None);
        await repo.SaveChangesAsync(CancellationToken.None);

        var all = await db.RefreshTokens.ToListAsync();
        all.Should().HaveCount(1);

        var stored = all.Single();
        stored.Token.Should().Be("tok-xyz");
    }

    [Fact]
    public async Task AddAsync_Should_NotSave_Without_SaveChangesAsync()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var repo = new RefreshTokenRepository(db);

        await repo.AddAsync(new RefreshToken
        {
            UserId = Guid.NewGuid(),
            Token = "unsaved-token",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
        });

        var all = await db.RefreshTokens.ToListAsync();
        all.Should().BeEmpty("EF InMemory does not persist without SaveChangesAsync");
    }

    [Fact]
    public async Task RevokeAsync_Should_SetRevokedTrue_ForExistingToken()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "ttt",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
            Revoked = false
        };

        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync();

        var repo = new RefreshTokenRepository(db);

        await repo.RevokeAsync(token.Id);
        await repo.SaveChangesAsync();

        var updated = await db.RefreshTokens.FindAsync(token.Id);

        updated!.Revoked.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeAsync_Should_DoNothing_WhenTokenNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var repo = new RefreshTokenRepository(db);

        var id = Guid.NewGuid();

        await repo.RevokeAsync(id);
        await repo.SaveChangesAsync();

        var all = await db.RefreshTokens.ToListAsync();
        all.Should().BeEmpty();
    }

    [Fact]
    public async Task RevokeAllForUserAsync_Should_RevokeOnlyActiveTokens()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var userId = Guid.NewGuid();

        db.RefreshTokens.AddRange(
            new RefreshToken { UserId = userId, Token = "a", Revoked = false },
            new RefreshToken { UserId = userId, Token = "b", Revoked = false },
            new RefreshToken { UserId = userId, Token = "c", Revoked = true },  // already revoked
            new RefreshToken { UserId = Guid.NewGuid(), Token = "other", Revoked = false }
        );

        await db.SaveChangesAsync();

        var repo = new RefreshTokenRepository(db);

        await repo.RevokeAllForUserAsync(userId);
        await repo.SaveChangesAsync();

        var all = await db.RefreshTokens.Where(x => x.UserId == userId).ToListAsync();

        all.Should().HaveCount(3);

        all.Should().Contain(x => x.Token == "a" && x.Revoked);
        all.Should().Contain(x => x.Token == "b" && x.Revoked);
        all.Should().Contain(x => x.Token == "c" && x.Revoked, "already revoked token stays revoked");
    }
}
