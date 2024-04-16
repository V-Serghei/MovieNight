using System;
using System.Collections.Generic;
using MovieNight.Domain.enams;
using Newtonsoft.Json;

namespace MovieNight.Domain.Entities.MovieM
{
    public class MovieTemplateInfE
    {
        
        public int Id { get; set; }
        public string Title { get; set; }
        
        public FilmCategory Tags { get; set; }

        public string PosterImage { get; set; }
        public string Quote { get; set; }
        public string Description { get; set; }
        public DateTime ProductionYear { get; set; }
        public string Country { get; set; }
        public List<string> Genre { get; set; }
        public string Location { get; set; }
        public string Director { get; set; }
        public string DurationJ { get; set; }

        [JsonIgnore] 
        public DateTime Duration { get; set; }
        public float MovieNightGrade { get; set; }
        public string Certificate { get; set; }
        public string ProductionCompany { get; set; }
        public string Budget { get; set; }
        public string GrossWorldwide { get; set; }
        public string Language { get; set; }
        public List<CastMemberE> CastMembers { get; set; }
        public List<MovieCardE> MovieCards { get; set; }
        public List<InterestingFactE> InterestingFacts { get; set; }
    }

    
}