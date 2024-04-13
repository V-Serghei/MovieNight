using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Domain.Entities.UserId;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MovieNight.BusinessLogic.DBModel
{
    public class UserContext : DbContext
    {
        public UserContext() : base("name=MovieNight")
        {

        }

        public virtual DbSet<UserDbTable> UsersT { get; set; }
        public virtual DbSet<PEdBdTable> PEdBdTables { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserDbTable>()
                .HasOptional(u => u.PEdBdTable) // Указывает, что User может не иметь PEdBdTable
                .WithRequired(p => p.User); // Указывает, что PEdBdTable всегда связана с User
        }
       


    }
}
