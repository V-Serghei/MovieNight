using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.Domain.Entities.PersonalP
{
    public class PEdBdTable
    {


        [Key]
        [ForeignKey("User")]
        public int? UserDbTableId { get; set; } // Изменен тип на int?

        // Навигационное свойство
        public virtual UserDbTable User { get; set; }


        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string AboutMe { get; set; }

        public string Gender { get; set; }

        public string Avatar { get; set; }

        public DateTime DataBirth { get; set; }

        public string PhoneNumber { get; set; }

        public string Country { get; set; }

        public string Quote { get; set; }

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
