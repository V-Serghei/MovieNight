
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MovieNight.Web.Models.Different;
using MovieNight.Web.Models.Movie;

namespace MovieNight.Web.Models.PersonalP
{
    public class PersonalProfileModel
    {
        public string Avatar { get; set; }
        public UserModel BUserModel { get; set; }
        public string Quote { get; set; }
        public string AboutMe { get; set; }
        public PhoneNumModel Number { get; set; }
        public string Location { get; set; }
        //some type for world view statistics
        public List<ViewingHistoryModel> ViewingHistory { get; set; }
        public List<ListOfFilmsModel> ListInThePlans { get; set; }
    }
}