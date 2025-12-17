using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Media.Domain.Entities;
using Media.Infrastructure.Data;
using Media.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Media.Tests;

public class MediaRepositoryTests
{
    private static MediaDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<MediaDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new MediaDbContext(options);
    }

    [Fact]
    public async Task AddAsync_Should_PersistMediaFile_AndReturnSameInstance()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        var repo = new MediaRepository(db);

        var file = new MediaFile
        {
            Id = Guid.NewGuid(),
            FileName = "poster.jpg",
            ContentType = "image/jpeg",
            Length = 12345,
            Data = new byte[] { 1, 2, 3, 4 },
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = await repo.AddAsync(file, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(file); // репозиторий возвращает тот же объект

        var stored = await db.MediaFiles.SingleOrDefaultAsync(x => x.Id == file.Id);
        stored.Should().NotBeNull();
        stored!.FileName.Should().Be("poster.jpg");
        stored.ContentType.Should().Be("image/jpeg");
        stored.Length.Should().Be(12345);
        stored.Data.Should().Equal(new byte[] { 1, 2, 3, 4 });
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNull_WhenFileNotExists()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        var repo = new MediaRepository(db);

        // Act
        var result = await repo.GetAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_Should_ReturnFile_WhenExists()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var id = Guid.NewGuid();
        var file = new MediaFile
        {
            Id = id,
            FileName = "trailer.mp4",
            ContentType = "video/mp4",
            Length = 999_999,
            Data = new byte[] { 10, 20, 30 },
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.MediaFiles.Add(file);
        await db.SaveChangesAsync();

        var repo = new MediaRepository(db);

        // Act
        var result = await repo.GetAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.FileName.Should().Be("trailer.mp4");
        result.ContentType.Should().Be("video/mp4");
        result.Length.Should().Be(999_999);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNotTrackedEntity()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();

        // 1) Сначала контекст для сидирования данных
        await using (var seedDb = CreateContext(dbName))
        {
            var id = Guid.NewGuid();
            var file = new MediaFile
            {
                Id = id,
                FileName = "original.png",
                ContentType = "image/png",
                Length = 100,
                Data = new byte[] { 42 },
                CreatedAt = DateTimeOffset.UtcNow
            };

            seedDb.MediaFiles.Add(file);
            await seedDb.SaveChangesAsync();
        }

        // 2) Новый контекст для чтения (важно!)
        await using var db = CreateContext(dbName);
        var repo = new MediaRepository(db);

        // Act
        var fromRepo = await repo.GetAsync(db.MediaFiles.Select(x => x.Id).First(), CancellationToken.None);

        // Assert
        fromRepo.Should().NotBeNull();

        // EF не должен начинать трекать сущность, возвращённую AsNoTracking-запросом
        db.ChangeTracker.Entries<MediaFile>().Should().BeEmpty("GetAsync использует AsNoTracking и не должен трекать сущности");
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveFile_WhenExists()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        var id = Guid.NewGuid();
        var file = new MediaFile
        {
            Id = id,
            FileName = "to-delete.bin",
            ContentType = "application/octet-stream",
            Length = 50,
            Data = new byte[] { 5, 6, 7 },
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.MediaFiles.Add(file);
        await db.SaveChangesAsync();

        var repo = new MediaRepository(db);

        // Act
        await repo.DeleteAsync(id, CancellationToken.None);

        // Assert
        var stored = await db.MediaFiles.SingleOrDefaultAsync(x => x.Id == id);
        stored.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_Should_DoNothing_WhenFileNotExists()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);

        // заранее добавим другой файл
        db.MediaFiles.Add(new MediaFile
        {
            Id = Guid.NewGuid(),
            FileName = "existing.dat",
            ContentType = "application/octet-stream",
            Length = 10,
            Data = new byte[] { 1 },
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var beforeCount = await db.MediaFiles.CountAsync();
        var repo = new MediaRepository(db);

        // Act
        await repo.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        var afterCount = await db.MediaFiles.CountAsync();
        afterCount.Should().Be(beforeCount, "удаление несуществующего файла не должно менять состояние БД");
    }
}
