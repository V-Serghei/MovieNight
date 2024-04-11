using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.Domain.Entities.DifferentE;

namespace MovieNight.Domain.Entities.MovieM
{
    public class ListOfFilms
    {
        public string Name { get; set; }
        public TimeD Date { get; set; }
        public long NumberOfViews { get; set; }
        public ICollection<Tag> Tags { get; set; }
        public int Star { get; set; }
    }
}
