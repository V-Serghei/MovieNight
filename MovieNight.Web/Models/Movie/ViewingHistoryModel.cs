using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieNight.Web.Models.Movie
{
    public class ViewingHistoryModel
    {
        public TimeModel ViewingTime { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public int? Id { get; set; }
        public int Star { get; set; }
        public PosterModel Poster { get; set; }
    }
}