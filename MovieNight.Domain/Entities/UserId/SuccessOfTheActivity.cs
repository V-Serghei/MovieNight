using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.Domain.Entities.PersonalP;

namespace MovieNight.Domain.Entities.UserId
{
    public class SuccessOfTheActivity
    {
        public ProfEditingE InfOfUser { get; set; }
        public string Msg { get; set; }
        public bool Successes { get; set; }
    }
}
