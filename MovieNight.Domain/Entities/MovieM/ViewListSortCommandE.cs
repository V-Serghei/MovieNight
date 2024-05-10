using System.Collections.Generic;
using MovieNight.Domain.enams;

namespace MovieNight.Domain.Entities.MovieM
{
    public class ViewListSortCommandE
    {
        public FilmCategory Category { get; set; }
        
        public SelectField Field { get; set; }
        
        public SortDirection SortingDirection { get; set; }
        
        public int PageNumber { get; set; }
        
        public Direction DirectionStep { get; set; }
        
        public ICollection<ViewingHistoryM> CurrentListViewing { get; set; }
        
        public string SearchParameter { get; set; }
    }
}