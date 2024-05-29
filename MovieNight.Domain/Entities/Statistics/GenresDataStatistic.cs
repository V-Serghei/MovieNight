using System.Collections.Generic;

namespace MovieNight.Domain.Entities.Statistics
{
    public class GenresDataStatistic
    {
        public List<int> CountGenreOrCountry { get; set; }
        public List<string> GenresOrCountry { get; set; }


        public GenresDataStatistic()
        {
            CountGenreOrCountry = new List<int>();
            GenresOrCountry = new List<string>();
        }
    }
}