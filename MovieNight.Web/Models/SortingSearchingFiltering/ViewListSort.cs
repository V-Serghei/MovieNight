using System.Collections.Generic;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Web.Models.Movie;

namespace MovieNight.Web.Models.SortingSearchingFiltering
{
    public class ViewListSort
    {
        public FilmCategory Category { get; set; }
        
        public SelectField Field { get; set; }
        
        public SortDirection SortingDirection { get; set; }
        
        public int PageNumber { get; set; }
        
        public Direction DirectionStep { get; set; }
        
        public List<ViewingHistoryModel> CurrentListViewing { get; set; }
        
        public string SearchParameter { get; set; }

        public ViewListSort()
        {
            CurrentListViewing = new List<ViewingHistoryModel>();
        }
        
    }
}