using MovieNight.Web.Models.Different;
using MovieNight.Web.Models.Movie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MovieNight.Web.Models.Achievement;

namespace MovieNight.Web.Models.Friends
{
    public class FriendPageModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Avatar { get; set; }
        public UserModel BUserE { get; set; }
        public DateTime DataBirth { get; set; }
        public string Quote { get; set; }
        public string AboutMe { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public List<ViewingHistoryModel> ViewingHistory { get; set; }
        public List<ListOfFilmsModel> ListInThePlans { get; set; }
        public string Country { get; set; }
        public bool YPICOBSBYF { get; set; }
        public bool SEOBIAY { get; set; }
        public bool HYBH { get; set; }
        public bool HMG { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Instagram { get; set; }
        public string GitHab { get; set; }
        public int AnimeCount { get; set; }
        public int AnimeTotal { get; set; }
        public int CartonsCount { get; set; }
        public int CartonTotal { get; set; }
        public int FilmCount { get; set; }
        public int FilTotal { get; set; }
        public int SerialsCount { get; set; }
        public int SerialTotal { get; set; }
        public string MsgResp { get; set; }
        
        public List<AchievementModel> Achievements { get; set; } = new List<AchievementModel>();
    }
}