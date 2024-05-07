using MovieNight.Domain.enams;

namespace MovieNight.Domain.Entities.MovieM.SearchParam
{
    public class FilmsCommandS
    {
        public FilmCategory Category { get; set; }
        public SortingOption SortPar { get; set; }
        public int PageNom { get; set; }
        public SortDirection SortingDirection { get; set; }
        public Direction Direction { get; set; }
        public int SkipParameter { get; set; }
    }
}