using MovieNight.Web.Models.Movie;
using System;
using System.Collections.Generic;

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