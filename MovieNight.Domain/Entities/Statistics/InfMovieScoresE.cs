using System.Collections.Generic;
using MovieNight.Domain.Entities.MovieM;

namespace MovieNight.Domain.Entities.Statistics
{
    public class InfMovieScoresE
    {
        public List<int> MyGrades { get; set; }
        public List<float> MovieNightGrade { get; set; }
        public List<string> TitleMovie { get; set; }
        public List<string> DataAddGrade { get; set; }
        public List<int?> IdMovie { get; set; }


        public InfMovieScoresE()
        {
            MyGrades = new List<int>();
            MovieNightGrade = new List<float>();
            TitleMovie = new List<string>();
            DataAddGrade = new List<string>();
            IdMovie = new List<int?>();
        }
    }
}