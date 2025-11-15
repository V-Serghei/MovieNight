using System.Text.Json;
using Messages.API.DTO;
using Messages.Domain.Reporitory;

namespace Messages.API.Endpoints;

public static class MessagesEndpoints
{
    public static IEndpointRouteBuilder MapMessagesEndpoints(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/messages").WithTags("Messages");
        
        g.MapGet("/", async (string senderId, IMessagesRepository repo, CancellationToken ct) =>
        {
            var all = await repo.FindBySenderIdAsync(senderId);
            return Results.Ok(all);
        });
        
        g.MapGet("/{id:guid}", async (Guid id, IMessagesRepository repo, CancellationToken ct) =>
        {
            var movie = await repo.FindByIdAsync(id, ct);
            return movie is not null ? Results.Ok(movie) : Results.NotFound();
        });
        
        g.MapGet("/{receiverId}", async (string receiverId, IMessagesRepository repo, CancellationToken ct) =>
        {
            var movie = await repo.FindByReceiverIdAsync(receiverId, ct);
            return movie is not null ? Results.Ok(movie) : Results.NotFound();
        });
        
        g.MapPost("/", async (MessagesDTO movieDto, IMessagesRepository repo, CancellationToken ct) =>
        {
            var movie = new Domain.Entities.Messages
            {
                IsChecked = false,
                SenderName = movieDto.SenderName,
                SenderId = movieDto.SenderId,
                RecipientName = movieDto.RecipientName,
                RecipientId = movieDto.RecipientId,
                Date = movieDto.Date,
                Message = movieDto.Message,
                Theme = movieDto.Theme,
                IsStarred = false
            };
            await repo.AddAsync(movie, ct);
            await repo.SaveChangesAsync(ct);
            return Results.Created($"/movies/{movie.Id}", movie);
        });
        return routes;
    }
}