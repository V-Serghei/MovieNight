using System;

namespace MovieNight.Web.Models.Friends
{
    public class ScoresFriendsGaveTheMovieModel
    {
        public string UserName { get; set; }
        public int Score { get; set; }
        public DateTime ReviewData { get; set; }
        public string ReviewDateString { get; set; }
    }
}