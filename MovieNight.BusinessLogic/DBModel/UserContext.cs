using MovieNight.Domain.Entities.UserId;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieNight.BusinessLogic.DBModel
{
    public class UserContext :DbContext
    {
        public UserContext():base ("name=MovieNight") 
        { 

        }

        public virtual DbSet<UserDbTable> UsersT { get; set; }
    }
}
