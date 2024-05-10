using MovieNight.Domain.Entities.MovieM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieNight.Domain.Entities.Friends
{
    public class FriendsPageD
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public string Avatar { get; set; }
        public UserE BUserE { get; set; }

        public DateTime DataBirth { get; set; }

        public string Quote { get; set; }
        public string AboutMe { get; set; }

        public string PhoneNumber { get; set; }

        public string Location { get; set; }
        public List<ViewingHistoryM> ViewingHistory { get; set; }
        public List<ListOfFilmsE> ListInThePlans { get; set; }

        public string Country { get; set; }

        public bool YPICOBSBYF { get; set; }

        public bool SEOBIAY { get; set; }

        public bool HYBH { get; set; }

        public bool HMG { get; set; }

        public string Facebook { get; set; }

        public string Twitter { get; set; }

        public string Instagram { get; set; }

        public string Skype { get; set; }
    }
}
