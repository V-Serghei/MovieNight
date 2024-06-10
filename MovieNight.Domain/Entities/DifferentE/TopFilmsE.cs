using System;
using System.Collections.Generic;
using MovieNight.Domain.enams;

namespace MovieNight.Domain.Entities.DifferentE
{
    public class TopFilmsE
    {
        public int? MovieId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public long NumberOfViews { get; set; }
        public FilmCategory Category { get; set; }
        public float Star { get; set; }
        public List<string> Genre { get; set; }
        public string PosterImage { get; set; }
    }
}