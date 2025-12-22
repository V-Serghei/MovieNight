using Friends.Domain.Enums;

namespace Friends.Domain.Entities;

public class Friends
{
    public string IdUser { get; set; }
    public string IdFriend { get; set; }
    public Friendship KindOfFriendship { get; set; }
}