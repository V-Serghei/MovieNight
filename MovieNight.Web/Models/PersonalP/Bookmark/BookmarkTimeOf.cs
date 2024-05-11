using System.Collections.Generic;
using System.Linq;
using MovieNight.Web.Models.Movie;

namespace MovieNight.Web.Models.PersonalP.Bookmark
{
    public class BookmarkTimeOf
    {
        public BookmarkTimeOf()
        {
            Bookmark = new List<BookmarkModel>();
            MovieInTimeOfBookmark = new List<MovieTemplateInfModel>();
        }

        public ICollection<BookmarkModel> Bookmark { get; set; }
        public ICollection<MovieTemplateInfModel> MovieInTimeOfBookmark { get; set; }
        
        public void SortByTimeAdd()
        {
            var sortedBookmarks = Bookmark.OrderByDescending(b => b.TimeAdd).ToList();
            var sortedMovies = new List<MovieTemplateInfModel>();

            foreach (var bookmark in sortedBookmarks)
            {
                var movie = MovieInTimeOfBookmark.FirstOrDefault(m => m.Id == bookmark.IdMovie);
                if (movie != null)
                {
                    sortedMovies.Add(movie);
                }
            }

            Bookmark = sortedBookmarks;
            MovieInTimeOfBookmark = sortedMovies;
        }
    }
}