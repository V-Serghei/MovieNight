using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieNight.Web.Models.Movie
{
    public class ViewingHistoryModel
    {
        
        public string ReviewDate { get; set; }
        
        public string YearOfRelease { get; set; }
        
        public string Description { get; set; }
        public string Title { get; set; }
        public int? Id { get; set; }
        public int UserValues { get; set; }
        
        public float MovieNightGrade { get; set; }
        public string Poster { get; set; }
        
        public string UserComment { get; set; } 
        
        public int UserViewCount { get; set; }        
        
        public string TimeSpent { get; set; }



    }
}