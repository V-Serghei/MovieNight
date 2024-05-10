using System.Collections.Generic;

namespace MovieNight.Domain.Entities.Friends
{
    public class FriendsListD
    { 
        public FriendsListD()
        {
            ListOfFriends = new List<FriendsPageD>();
        }
        public List<FriendsPageD> ListOfFriends { get; set; }
    }
}