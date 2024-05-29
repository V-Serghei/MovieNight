using MovieNight.Web.Models.Movie;
using System;
using System.Collections.Generic;
using MovieNight.Web.Models.Different;

namespace MovieNight.Web.Models.Friends
{
    public class FriendListModel
    {
        public FriendListModel()
        {
            ListOfFriends = new List<FriendPageModel>();
        }
        public List<FriendPageModel> ListOfFriends { get; set; }
        
    }
}