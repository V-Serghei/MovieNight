using System.Text.Json;
using Microsoft.AspNetCore.Routing;
using Review.Domain.Entities;
using Review.Domain.Repository.Review;
using Review.API.DTO;

namespace Review.API.Endpoints;

public static class ReviewEndpoints
{
    public static IEndpointRouteBuilder MapReviewEndpoints(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/reviews").WithTags("Reviews");
        //Получить все отзывы по фильму
        g.MapGet("/{filmId}", async (string filmId, IReviewRepository repo, CancellationToken ct) =>
        {
            var reviews = await repo.FindByFilmIdAsync(filmId, ct);
            return reviews is not null ? Results.Ok(reviews) : Results.NotFound();
        });
        
        g.MapPost("/", async (ReviewDTO reviewDto, IReviewRepository repo, CancellationToken ct) =>
        {
            var review = new Domain.Entities.Review
            {
                Id = Guid.NewGuid(),
                FilmId = reviewDto.FilmId,
                Film = reviewDto.Film,
                UserId = reviewDto.UserId,
                User =  reviewDto.User,
                Text = reviewDto.Text,
                Date = reviewDto.Date
            };
            await repo.AddAsync(review, ct);
            await repo.SaveChangesAsync(ct);
            return Results.Created($"/reviews/{review.Id}", review);
        });
        return routes;
    }
}