using System;
using System.Collections.Generic;

namespace MovieNight.Web.Models.Movie
{
    public class MovieTemplateInfModel
    {
        public string Title { get; set; }
        public string PosterImage { get; set; }
        public string Quote { get; set; }
        public string Description { get; set; }
        public DateTime ProductionYear { get; set; }
        public string Country { get; set; }
        public List<string>Genre { get; set; }
        public string Location { get; set; }
        public string Director { get; set; }
        public int Duration { get; set; }
        public float MovieNightGrade { get; set; }
        public string Certificate { get; set; }
        public string ProductionCompany { get; set; }
        public string Budget { get; set; }
        public string GrossWorldwide { get; set; }
        public DateTime TimeV { get; set; }
        public string Language { get; set; }
        public List<CastMember> CastMembers { get; set; }
        public List<MovieCard> MovieCards { get; set; }
        public List<InterestingFact> InterestingFacts { get; set; }
    }
    
}