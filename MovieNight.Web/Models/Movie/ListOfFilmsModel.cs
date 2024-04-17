using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MovieNight.Domain.enams;
using MovieNight.Web.Models.Different;

namespace MovieNight.Web.Models.Movie
{
    public class ListOfFilmsModel
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public long NumberOfViews { get; set; }
        public FilmCategory Tags { get; set; }
        public float Star { get; set; }
        
        public string Genre { get; set; }
    }
}