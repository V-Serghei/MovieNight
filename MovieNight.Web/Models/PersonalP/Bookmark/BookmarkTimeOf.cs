using System.Collections.Generic;
using MovieNight.Web.Models.Movie;

namespace MovieNight.Web.Models.PersonalP.Bookmark
{
    public class BookmarkTimeOf
    {
        public ICollection<BookmarkModel> Bookmark { get; set; }
        public ICollection<MovieTemplateInfModel> MovieInTimeOfBookmark { get; set; }
    }
}