using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieNight.Domain.Entities.MovieM
{
    public class ViewingHistoryM
    {
        public DateTime ViewingTime { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public int? Id { get; set; }
        public int Star { get; set; }
        public Poster Poster { get; set; }

    }
}
