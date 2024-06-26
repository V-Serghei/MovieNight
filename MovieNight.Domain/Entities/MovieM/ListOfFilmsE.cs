﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.DifferentE;

namespace MovieNight.Domain.Entities.MovieM
{
    public class ListOfFilmsE
    {
        public int? MovieId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public long NumberOfViews { get; set; }
        public FilmCategory Category { get; set; }
        public float Star { get; set; }
        
        public string Genre { get; set; }

    }
}
