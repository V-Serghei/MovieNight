using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MovieNight.Web.Models.Different;

namespace MovieNight.Web.Models.Movie
{
    public class ListOfFilmsModel
    {
        public string Name { get; set; }
        public TimeModel Date { get; set; }
        public long NumberOfViews { get; set; }
        public ICollection<TagModel> Tags { get; set; }
        public int Star { get; set; }
    }
}