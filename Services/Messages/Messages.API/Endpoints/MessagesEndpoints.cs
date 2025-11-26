using System.Text.Json;
using Messages.API.DTO;
using Messages.Domain.Reporitory;

namespace Messages.API.Endpoints;

public static class MessagesEndpoints
{
    public static IEndpointRouteBuilder MapMessagesEndpoints(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/messages").WithTags("Messages");
        
        g.MapGet("/sent/{senderId}", async (string senderId, IMessagesRepository repo, CancellationToken ct) =>
        {
            var all = await repo.FindBySenderIdAsync(senderId);
            return Results.Ok(all);
        });
        
        g.MapGet("/{id:guid}", async (Guid id, IMessagesRepository repo, CancellationToken ct) =>
        {
            var movie = await repo.FindByIdAsync(id, ct);
            return movie is not null ? Results.Ok(movie) : Results.NotFound();
        });
        
        g.MapGet("/by-receiver/{receiverId}", async (string receiverId, IMessagesRepository repo, CancellationToken ct) =>
        {
            var messages = await repo.FindByReceiverIdAsync(receiverId, ct);
            return Results.Ok(messages);
        });
        
        g.MapPost("/compose", async (MessagesDTO messagesDto, IMessagesRepository repo, CancellationToken ct) =>
        {
            var messages = new Domain.Entities.Messages
            {
                IsChecked = false,
                SenderName = messagesDto.SenderName,
                SenderId = messagesDto.SenderId,
                RecipientName = messagesDto.RecipientName,
                RecipientId = messagesDto.RecipientId,
                Date = messagesDto.Date,
                Message = messagesDto.Message,
                Theme = messagesDto.Theme,
                IsStarred = false
            };
            await repo.AddAsync(messages, ct);
            await repo.SaveChangesAsync(ct);
            return Results.Created($"/messages/{messages.Id}", messagesDto);
        });
        return routes;
    }
}