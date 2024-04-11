using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieNight.Web.Models.Movie
{
    public class PosterModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; } = string.Empty;
    }
}