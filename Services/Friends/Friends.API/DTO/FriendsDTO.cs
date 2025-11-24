using Friends.Domain.Enums;

namespace Friends.API.DTO;

public record FriendsDTO(
    string IdUser,
    string IdFriend, 
    Friendship KindOfFriendship
);