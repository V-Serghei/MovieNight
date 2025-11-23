namespace Friends.API.DTO;

public class FriendsDTO
{
    public Guid Id { get; set; } = Guid.NewGuid();
    string IdUser { get; set; }
    string IdFriend { get; set; }
    
}