using System.Collections.Generic;
using MovieNight.Web.Models.Movie;

namespace MovieNight.Web.Models
{
    public class StatisticModel
    {
        public int AnimeCount { get; set; }
        public int AnimeTotal { get; set; }
        public int CartonsCount { get; set; }
        public int CartonTotal { get; set; }
        public int FilmCount { get; set; }
        public int FilTotal { get; set; }
        public int SerialsCount { get; set; }
        public int SerialTotal { get; set; }
        
        public int ViewingCount { get; set; }
        public int BookmarkCount { get; set; }
        
        
        public int GenreTotal { get; set; }
        public int YourGenrePrefer { get; set; }
        
        public int CountryTotal { get; set; }
        public int YourCountryPrefer { get; set; }
        
        public int YourGradeCount { get; set; }
        public double YourAverageRating { get; set; }
        public int YourMostGrade { get; set; }
        
        
        public List<ViewingHistoryModel> ViewList { get; set; }

        public StatisticModel()
        {
            ViewList = new List<ViewingHistoryModel>();
        }
        
        
    }
}