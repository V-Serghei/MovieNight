using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bookmark.Domain.Entity;
using Bookmark.Infrastructure.Data;
using Bookmark.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Bookmark.Tests
{
    public class BookmarksRepositoryTests
    {
        private BookmarksDbContext CreateContext(string name)
        {
            var options = new DbContextOptionsBuilder<BookmarksDbContext>()
                .UseInMemoryDatabase(name)
                .Options;

            return new BookmarksDbContext(options);
        }

        private Domain.Entity.Bookmark CreateBookmark(Guid? id = null, Guid? user = null, Guid? movie = null, BookmarkKind kind = BookmarkKind.Normal)
        {
            return new Domain.Entity.Bookmark
            {
                Id = id ?? Guid.NewGuid(),
                UserId = user ?? Guid.NewGuid(),
                MovieId = movie ?? Guid.NewGuid(),
                Kind = kind,
                MovieTitle = "Test Movie",
                MovieYear = 2020,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        [Fact]
        public async Task GetByUserAsync_Should_ReturnOnlyUserBookmarks_AndOrdered()
        {
            var db = CreateContext(Guid.NewGuid().ToString());

            var userId = Guid.NewGuid();
            var repo = new BookmarksRepository(db);

            var b1 = CreateBookmark(user: userId);
            b1.CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10);

            var b2 = CreateBookmark(user: userId);
            b2.CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1);

            var other = CreateBookmark(user: Guid.NewGuid());

            db.Bookmarks.AddRange(b1, b2, other);
            await db.SaveChangesAsync();

            // Act
            var result = await repo.GetByUserAsync(userId, null, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.First().Id.Should().Be(b2.Id); // most recent first
            result.Last().Id.Should().Be(b1.Id);
        }

        [Fact]
        public async Task GetByUserAsync_Should_FilterByKind_WhenProvided()
        {
            var db = CreateContext(Guid.NewGuid().ToString());
            var repo = new BookmarksRepository(db);
            var userId = Guid.NewGuid();

            var normal = CreateBookmark(user: userId, kind: BookmarkKind.Normal);
            var temporary = CreateBookmark(user: userId, kind: BookmarkKind.Temporary);

            db.Bookmarks.AddRange(normal, temporary);
            await db.SaveChangesAsync();

            var result = await repo.GetByUserAsync(userId, BookmarkKind.Temporary, CancellationToken.None);

            result.Should().ContainSingle();
            result.Single().Kind.Should().Be(BookmarkKind.Temporary);
        }

        [Fact]
        public async Task FindAsync_Should_ReturnBookmark_WhenExists()
        {
            var db = CreateContext(Guid.NewGuid().ToString());
            var repo = new BookmarksRepository(db);

            var userId = Guid.NewGuid();
            var movieId = Guid.NewGuid();

            var b = CreateBookmark(user: userId, movie: movieId, kind: BookmarkKind.Normal);

            db.Bookmarks.Add(b);
            await db.SaveChangesAsync();

            var result = await repo.FindAsync(userId, movieId, BookmarkKind.Normal, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(b.Id);
        }

        [Fact]
        public async Task FindAsync_Should_ReturnNull_WhenNotExists()
        {
            var db = CreateContext(Guid.NewGuid().ToString());
            var repo = new BookmarksRepository(db);

            var result = await repo.FindAsync(Guid.NewGuid(), Guid.NewGuid(), BookmarkKind.Normal, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_Should_InsertBookmark()
        {
            var db = CreateContext(Guid.NewGuid().ToString());
            var repo = new BookmarksRepository(db);

            var b = CreateBookmark();

            await repo.AddAsync(b, CancellationToken.None);
            await repo.SaveChangesAsync(CancellationToken.None);

            db.Bookmarks.Should().ContainSingle();
            db.Bookmarks.First().Id.Should().Be(b.Id);
        }

        [Fact]
        public async Task RemoveAsync_Should_DeleteBookmark()
        {
            var db = CreateContext(Guid.NewGuid().ToString());
            var repo = new BookmarksRepository(db);

            var b = CreateBookmark();

            db.Bookmarks.Add(b);
            await db.SaveChangesAsync();

            await repo.RemoveAsync(b, CancellationToken.None);
            await repo.SaveChangesAsync(CancellationToken.None);

            db.Bookmarks.Should().BeEmpty();
        }

        [Fact]
        public async Task RemoveByMovieAsync_Should_DeleteAllMatching()
        {
            var db = CreateContext(Guid.NewGuid().ToString());
            var repo = new BookmarksRepository(db);

            var uid = Guid.NewGuid();
            var mid = Guid.NewGuid();

            var one = CreateBookmark(user: uid, movie: mid);
            var two = CreateBookmark(user: uid, movie: mid);
            var other = CreateBookmark(user: uid, movie: Guid.NewGuid());

            db.Bookmarks.AddRange(one, two, other);
            await db.SaveChangesAsync();

            await repo.RemoveByMovieAsync(uid, mid, CancellationToken.None);
            await repo.SaveChangesAsync(CancellationToken.None);

            db.Bookmarks.Should().ContainSingle();
            db.Bookmarks.First().Id.Should().Be(other.Id);
        }

        [Fact]
        public async Task RemoveExpiredTemporaryAsync_Should_RemoveExpiredOnly()
        {
            var db = CreateContext(Guid.NewGuid().ToString());
            var repo = new BookmarksRepository(db);

            var now = DateTimeOffset.UtcNow;

            var expired = CreateBookmark(kind: BookmarkKind.Temporary);
            expired.ExpiresAt = now.AddMinutes(-1);

            var fresh = CreateBookmark(kind: BookmarkKind.Temporary);
            fresh.ExpiresAt = now.AddMinutes(10);

            var normal = CreateBookmark(kind: BookmarkKind.Normal); // should NOT be deleted

            db.Bookmarks.AddRange(expired, fresh, normal);
            await db.SaveChangesAsync();

            await repo.RemoveExpiredTemporaryAsync(now, CancellationToken.None);
            await repo.SaveChangesAsync(CancellationToken.None);

            db.Bookmarks.Should().HaveCount(2);
            db.Bookmarks.Should().Contain(fresh);
            db.Bookmarks.Should().Contain(normal);
        }

        [Fact]
        public async Task SaveChangesAsync_Should_Save()
        {
            var db = CreateContext(Guid.NewGuid().ToString());
            var repo = new BookmarksRepository(db);

            var b = CreateBookmark();

            await repo.AddAsync(b, CancellationToken.None);
            await repo.SaveChangesAsync(CancellationToken.None);

            db.Bookmarks.Should().ContainSingle();
        }
    }
}
