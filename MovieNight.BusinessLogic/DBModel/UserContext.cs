using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Domain.Entities.UserId;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieNight.Domain.Entities.PersonalP.PersonalPDb;


namespace MovieNight.BusinessLogic.DBModel
{
    public sealed class UserContext : DbContext
    {
        public UserContext() : base("name=MovieNight")
        {

        }

        public DbSet<UserDbTable> UsersT { get; set; }
        public DbSet<PEdBdTable> PEdBdTables { get; set; }
        
        public DbSet<ViewListDbTable> ViewList { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserDbTable>()
                .HasOptional(u => u.PEdBdTable)
                .WithRequired(p => p.User)
                .Map(m => m.MapKey("UserId"));

            modelBuilder.Entity<UserDbTable>()
                .HasMany(u => u.ViewList)
                .WithRequired(v => v.User)
                .HasForeignKey(v => v.UserId);
            
        }


       


    }
}
