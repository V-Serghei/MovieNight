using MovieNight.Domain.Entities.DifferentE;
using MovieNight.Domain.Entities.MovieM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.Domain.Entities.PersonalP
{
    public class PersonalInfoDbTable
    {

        public int Id { get; set; }

        public string UserId { get; set; }

        public string Avatar { get; set; }

        public UserDbTable BUserE { get; set; }

        public string Quote { get; set; }

        public string AboutMe { get; set; }

        public PhoneNumE Number { get; set; }

        public string Location { get; set; }

        //some type for world view statistics
        public List<ViewingHistoryM> ViewingHistory { get; set; }

        public List<ListOfFilms> ListInThePlans { get; set; }
    }
}
