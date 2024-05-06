using MovieNight.Domain.enams;

namespace MovieNight.Web.Models.Movie
{
    public class FilmCommandSort
    {
        public FilmCategory Category { get; set; }
        public SortingOption SortPar { get; set; }
        public int PageNom { get; set; }
        public SortDirection SortingDirection { get; set; }
        
    }
}