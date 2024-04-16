using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Domain.Entities.UserId;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.ModelBinding;
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
                .WithRequired(p => p.User);
            
            modelBuilder.Entity<ViewListDbTable>()
                .HasKey(w => w.Id); 
            
            modelBuilder.Entity<ViewListDbTable>()
                .HasRequired(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId);

            modelBuilder.Entity<ViewListDbTable>()
                .HasRequired(w => w.Movie)
                .WithMany()
                .HasForeignKey(w => w.MovieId);

            // modelBuilder.Entity<ViewListDbTable>()
            //     .HasOptional(v => v.User)
            //     .WithMany(u => u.ViewListEntries)
            //     .HasForeignKey(v => v.UserId);
            //
            // modelBuilder.Entity<ViewListDbTable>()
            //     .HasOptional(v => v.Movie)
            //     .WithMany(m => m.ViewListEntries)
            //     .HasForeignKey(v => v.MovieId);


            // modelBuilder.Entity<MovieDbTable>()
            //     .HasMany(m => m.CastMembers)
            //     .WithMany(c => c.Movies)
            //     .Map(mc =>
            //     {
            //         mc.ToTable("MovieCastMembers");
            //         mc.MapLeftKey("MovieId");
            //         mc.MapRightKey("CastMemberId");
            //     });

        }


       


    }
}
