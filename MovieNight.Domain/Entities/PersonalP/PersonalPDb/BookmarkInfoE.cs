using System;
using MovieNight.Domain.enams;

namespace MovieNight.Domain.Entities.PersonalP.PersonalPDb
{
    public class BookmarkInfoE
    {
        public int? MovieId { get; set; }
        public string Title { get; set; }
        public DateTime YearOfRelease { get; set; }
        public DateTime BookmarkDate { get; set; }
        public FilmCategory Category { get; set; }
        public float OverallRating { get; set; }
    }
}