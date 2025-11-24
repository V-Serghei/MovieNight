using Friends.API.DTO;
using Friends.Domain.Repository;

namespace Friends.API.Endpoints;

public static class FriendsEndpoints
{
    public static IEndpointRouteBuilder MapFriendsEndpoints(this IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/friends").WithTags("Friends");
        
        g.MapGet("/{userId}", async (string userId, IFriendsRepository repo) =>
        {
            var all = await repo.FindFriendsByUserIdAsync(userId);
            return all is not null ? Results.Ok(all) : Results.NotFound();
        });
        
        g.MapPost("/", async (FriendsDTO friendsDto, IFriendsRepository repo, CancellationToken ct) =>
        {
            var friend = new Domain.Entities.Friends
            {
                IdUser = friendsDto.IdUser,
                IdFriend = friendsDto.IdFriend,
                KindOfFriendship = friendsDto.KindOfFriendship
            };
            await repo.AddAsync(friend, ct);
            await repo.SaveChangesAsync(ct);
            return Results.Created($"/{friend.IdFriend}", friend);
        });
        return routes;
    }
}