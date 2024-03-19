using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.Domain.Entities.DifferentE;
using MovieNight.Domain.Entities.MovieM;

namespace MovieNight.Domain.Entities.PersonalP
{
    public class PersonalProfileM
    {
        public string Avatar { get; set; }
        public UserE BUserE { get; set; }
        public string Quote { get; set; }
        public string AboutMe { get; set; }
        public PhoneNumE Number { get; set; }
        public string Location { get; set; }
        //some type for world view statistics
        public List<ViewingHistoryM> ViewingHistory { get; set; }
        public List<ListOfFilms> ListInThePlans { get; set; }
    }
}
