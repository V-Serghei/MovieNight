using System.Security.Claims;
using Bookmark.API.DTO;
using Bookmark.Domain.Entity;
using Bookmark.Domain.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Bookmark.API.Endpoints;

public static class BookmarksEndpoints
{
    public static IEndpointRouteBuilder MapBookmarksEndpoints(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/bookmarks")
            .WithTags("Bookmarks");

        g.MapGet("/", GetNormal);
        g.MapPost("/", AddNormal);
        g.MapDelete("/{movieId:guid}", DeleteNormal);

        g.MapGet("/temp", GetTemporary);
        g.MapPost("/temp", AddTemporary);
        g.MapDelete("/temp/{movieId:guid}", DeleteTemporary);

        g.MapGet("/watched", GetWatched);
        g.MapPost("/watched", AddOrUpdateWatched);
        g.MapDelete("/watched/{movieId:guid}", DeleteWatched);

        g.MapDelete("/movie/{movieId:guid}/all", DeleteAllKinds);

        return routes;
    }


    private static bool TryGetUserId(HttpContext http, out Guid userId)
    {
        if (http.Request.Headers.TryGetValue("X-UserId", out var fromHeader) &&
            Guid.TryParse(fromHeader.ToString(), out userId))
        {
            return true;
        }

        var claim = http.User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    http.User?.FindFirstValue("sub");

        if (Guid.TryParse(claim, out userId))
            return true;

        userId = Guid.Empty;
        return false;
    }

    private static BookmarkResponse ToResponse(Domain.Entity.Bookmark b) =>
        new(
            b.Id,
            b.MovieId,
            b.Kind,
            b.CreatedAt,
            b.WatchedAt,
            b.ExpiresAt,
            new UIMovieDto(
                b.MovieId,
                b.MovieTitle,
                b.MovieYear,
                b.MovieDuration,
                b.MoviePosterImage
            )
        );


    private static async Task<IResult> GetNormal(
        HttpContext http,
        IBookmarksRepository repo,
        CancellationToken ct)
    {
        if (!TryGetUserId(http, out var userId))
            return Results.Unauthorized();

        var list = await repo.GetByUserAsync(userId, BookmarkKind.Normal, ct);
        return Results.Ok(list.Select(ToResponse));
    }

    private static async Task<IResult> AddNormal(
        HttpContext http,
        [FromBody] AddBookmarkRequest req,
        IBookmarksRepository repo,
        CancellationToken ct)
    {
        if (!TryGetUserId(http, out var userId))
            return Results.Unauthorized();

        var movie = req.Movie;

        var exists = await repo.FindAsync(userId, movie.Id, BookmarkKind.Normal, ct);
        if (exists is not null)
            return Results.NoContent();

        var entity = new Domain.Entity.Bookmark
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MovieId = movie.Id,
            Kind = BookmarkKind.Normal,
            MovieTitle = movie.Title,
            MovieYear = movie.Year,
            MovieDuration = movie.Duration,
            MoviePosterImage = movie.PosterImage,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await repo.AddAsync(entity, ct);
        await repo.SaveChangesAsync(ct);
        return Results.Ok(ToResponse(entity));
    }

    private static async Task<IResult> DeleteNormal(
        HttpContext http,
        [FromRoute] Guid movieId,
        IBookmarksRepository repo,
        CancellationToken ct)
    {
        if (!TryGetUserId(http, out var userId))
            return Results.Unauthorized();

        var entity = await repo.FindAsync(userId, movieId, BookmarkKind.Normal, ct);
        if (entity is null)
            return Results.NoContent();

        await repo.RemoveAsync(entity, ct);
        await repo.SaveChangesAsync(ct);
        return Results.NoContent();
    }
    
    private static async Task<IResult> GetTemporary(
        HttpContext http,
        IBookmarksRepository repo,
        CancellationToken ct)
    {
        if (!TryGetUserId(http, out var userId))
            return Results.Unauthorized();

        var now = DateTimeOffset.UtcNow;
        await repo.RemoveExpiredTemporaryAsync(now, ct);
        await repo.SaveChangesAsync(ct);

        var list = await repo.GetByUserAsync(userId, BookmarkKind.Temporary, ct);
        list = list
            .Where(x => x.ExpiresAt == null || x.ExpiresAt > now)
            .ToList();

        return Results.Ok(list.Select(ToResponse));
    }

    private static async Task<IResult> AddTemporary(
        HttpContext http,
        [FromBody] AddTemporaryBookmarkRequest req,
        IBookmarksRepository repo,
        CancellationToken ct)
    {
        if (!TryGetUserId(http, out var userId))
            return Results.Unauthorized();

        var movie = req.Movie;
        var ttlMinutes = req.TtlMinutes ?? 24 * 60;
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(ttlMinutes);

        var existing = await repo.FindAsync(userId, movie.Id, BookmarkKind.Temporary, ct);
        if (existing is not null)
        {
            existing.ExpiresAt = expires;
            existing.MovieTitle = movie.Title;
            existing.MovieYear = movie.Year;
            existing.MovieDuration = movie.Duration;
            existing.MoviePosterImage = movie.PosterImage;
        }
        else
        {
            var entity = new Domain.Entity.Bookmark
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                MovieId = movie.Id,
                Kind = BookmarkKind.Temporary,
                MovieTitle = movie.Title,
                MovieYear = movie.Year,
                MovieDuration = movie.Duration,
                MoviePosterImage = movie.PosterImage,
                CreatedAt = now,
                ExpiresAt = expires
            };
            await repo.AddAsync(entity, ct);
        }

        await repo.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteTemporary(
        HttpContext http,
        [FromRoute] Guid movieId,
        IBookmarksRepository repo,
        CancellationToken ct)
    {
        if (!TryGetUserId(http, out var userId))
            return Results.Unauthorized();

        var entity = await repo.FindAsync(userId, movieId, BookmarkKind.Temporary, ct);
        if (entity is null)
            return Results.NoContent();

        await repo.RemoveAsync(entity, ct);
        await repo.SaveChangesAsync(ct);
        return Results.NoContent();
    }


    private static async Task<IResult> GetWatched(
        HttpContext http,
        IBookmarksRepository repo,
        CancellationToken ct)
    {
        if (!TryGetUserId(http, out var userId))
            return Results.Unauthorized();

        var list = await repo.GetByUserAsync(userId, BookmarkKind.Watched, ct);

        list = list
            .OrderByDescending(x => x.WatchedAt ?? x.CreatedAt)
            .ToList();

        return Results.Ok(list.Select(ToResponse));
    }

    private static async Task<IResult> AddOrUpdateWatched(
        HttpContext http,
        [FromBody] AddWatchedRequest req,
        IBookmarksRepository repo,
        CancellationToken ct)
    {
        if (!TryGetUserId(http, out var userId))
            return Results.Unauthorized();

        var movie = req.Movie;
        var when = req.WatchedAt ?? DateTimeOffset.UtcNow;

        var existing = await repo.FindAsync(userId, movie.Id, BookmarkKind.Watched, ct);
        if (existing is null)
        {
            var entity = new Domain.Entity.Bookmark
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                MovieId = movie.Id,
                Kind = BookmarkKind.Watched,
                MovieTitle = movie.Title,
                MovieYear = movie.Year,
                MovieDuration = movie.Duration,
                MoviePosterImage = movie.PosterImage,
                CreatedAt = when,
                WatchedAt = when
            };
            await repo.AddAsync(entity, ct);
        }
        else
        {
            existing.WatchedAt = when;
            existing.MovieTitle = movie.Title;
            existing.MovieYear = movie.Year;
            existing.MovieDuration = movie.Duration;
            existing.MoviePosterImage = movie.PosterImage;
        }

        await repo.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteWatched(
        HttpContext http,
        [FromRoute] Guid movieId,
        IBookmarksRepository repo,
        CancellationToken ct)
    {
        if (!TryGetUserId(http, out var userId))
            return Results.Unauthorized();

        var entity = await repo.FindAsync(userId, movieId, BookmarkKind.Watched, ct);
        if (entity is null)
            return Results.NoContent();

        await repo.RemoveAsync(entity, ct);
        await repo.SaveChangesAsync(ct);
        return Results.NoContent();
    }


    private static async Task<IResult> DeleteAllKinds(
        HttpContext http,
        [FromRoute] Guid movieId,
        IBookmarksRepository repo,
        CancellationToken ct)
    {
        if (!TryGetUserId(http, out var userId))
            return Results.Unauthorized();

        await repo.RemoveByMovieAsync(userId, movieId, ct);
        await repo.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}