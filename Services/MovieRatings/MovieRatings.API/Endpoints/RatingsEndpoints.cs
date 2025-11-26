using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MovieRatings.API.DTO;
using MovieRatings.Domain.Entity;
using MovieRatings.Domain.Repository;

namespace MovieRatings.API.Endpoints;

public static class RatingsEndpoints
{
    public static IEndpointRouteBuilder MapRatingsEndpoints(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/ratings").WithTags("Ratings");

        g.MapGet("/movies/{movieId:guid}", GetMovieRatingSummary);
        g.MapGet("/movies", GetMovieRatingSummaries);

        g.MapPut("/movies/{movieId:guid}", SetRating);
        g.MapDelete("/movies/{movieId:guid}", DeleteRating);

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


    private static MovieRatingSummaryDto ToDto(
        Guid movieId,
        MovieRatingAggregate? agg,
        Domain.Entity.MovieRatings? userRating)
    {
        return new MovieRatingSummaryDto(
            movieId,
            agg?.AverageScore,
            agg?.RatingsCount ?? 0,
            userRating?.Score
        );
    }

    private static async Task<IResult> GetMovieRatingSummary(
        HttpContext http,
        [FromRoute] Guid movieId,
        IRatingsRepository repo,
        CancellationToken ct)
    {
        TryGetUserId(http, out var userId);
        var agg = await repo.GetAggregateAsync(movieId, ct);
        var userRating = userId != Guid.Empty
            ? await repo.GetUserRatingAsync(userId, movieId, ct)
            : null;

        var dto = ToDto(movieId, agg, userRating);
        return Results.Ok(dto);
    }

    private static async Task<IResult> GetMovieRatingSummaries(
        HttpContext http,
        [FromQuery] string ids,
        IRatingsRepository repo,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(ids))
            return Results.BadRequest(new { error = "ids query param is required" });

        var movieIds = ids
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => Guid.TryParse(x, out var g) ? g : (Guid?)null)
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .Distinct()
            .ToList();

        if (movieIds.Count == 0)
            return Results.Ok(Array.Empty<MovieRatingSummaryDto>());

        TryGetUserId(http, out var userId);

        var (aggs, userRatings) = await repo.GetSummariesAsync(
            userId != Guid.Empty ? userId : null,
            movieIds,
            ct);

        var aggDict = aggs.ToDictionary(a => a.MovieId);
        var userDict = userRatings.ToDictionary(r => r.MovieId);

        var result = movieIds.Select(id =>
        {
            aggDict.TryGetValue(id, out var agg);
            userDict.TryGetValue(id, out var ur);
            return ToDto(id, agg, ur);
        });

        return Results.Ok(result);
    }

    private static async Task<IResult> SetRating(
        HttpContext http,
        [FromRoute] Guid movieId,
        [FromBody] SetRatingRequest req,
        IRatingsRepository repo,
        CancellationToken ct)
    {
        if (!TryGetUserId(http, out var userId))
            return Results.Unauthorized();

        if (req.Score < 1 || req.Score > 10)
            return Results.BadRequest(new { error = "Score must be between 1 and 10" });

        var (rating, agg) = await repo.SetRatingAsync(userId, movieId, req.Score, ct);
        await repo.SaveChangesAsync(ct);

        var dto = ToDto(movieId, agg, rating);
        return Results.Ok(dto);
    }

    private static async Task<IResult> DeleteRating(
        HttpContext http,
        [FromRoute] Guid movieId,
        IRatingsRepository repo,
        CancellationToken ct)
    {
        if (!TryGetUserId(http, out var userId))
            return Results.Unauthorized();

        var agg = await repo.DeleteRatingAsync(userId, movieId, ct);
        await repo.SaveChangesAsync(ct);

        var dto = ToDto(movieId, agg, null);
        return Results.Ok(dto);
    }
}