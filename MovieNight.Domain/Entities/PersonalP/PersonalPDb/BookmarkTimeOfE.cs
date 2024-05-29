using System.Collections.Generic;
using MovieNight.Domain.Entities.MovieM;

namespace MovieNight.Domain.Entities.PersonalP.PersonalPDb
{
    public class BookmarkTimeOfE
    {
        public BookmarkTimeOfE()
        {
            Bookmark = new List<BookmarkE>();
            MovieInTimeOfBookmark = new List<MovieTemplateInfE>();
        }
        
        public ICollection<BookmarkE> Bookmark { get; set; }
        public ICollection<MovieTemplateInfE> MovieInTimeOfBookmark { get; set; }
    }
}