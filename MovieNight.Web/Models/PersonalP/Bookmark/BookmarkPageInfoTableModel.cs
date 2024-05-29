using System;
using MovieNight.Domain.enams;

namespace MovieNight.Web.Models.PersonalP.Bookmark
{
    public class BookmarkPageInfoTableModel
    {
        public int? MovieId { get; set; }
        public string Title { get; set; }
        public string YearOfRelease { get; set; }
        public string BookmarkDate { get; set; }
        public FilmCategory Category { get; set; }
        public float OverallRating { get; set; }

    }
}