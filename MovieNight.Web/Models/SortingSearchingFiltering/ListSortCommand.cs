﻿using MovieNight.Domain.enams;

namespace MovieNight.Web.Models.SortingSearchingFiltering
{
    public class ListSortCommand
    {
        public int? UserId { get; set; }
        public int PageNumber { get; set; }
        public SelectField Field { get; set; }
        public Direction DirectionStep { get; set; }
        public SortDirection SortingDirection { get; set; }
        public string SearchParameter { get; set; }
        public FilmCategory Category { get; set; }
    }
}