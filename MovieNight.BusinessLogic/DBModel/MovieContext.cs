using System.Data.Entity;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.MovieM.EfDbEntities;

namespace MovieNight.BusinessLogic.DBModel
{
    public class MovieContext:DbContext
    {
        public MovieContext() : base("name=MovieNight")
        {
        }

        public DbSet<MovieDbTable> MovieDb { get; set; }
        
        public DbSet<MovieCardDbTable> MovieCard { get; set; }
        
        public DbSet<CastMemDbTable> CastDbTables { get; set; }
        
        public DbSet<InterestingFactDbTable> InterestingFact { get; set; }
 
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<MovieDbTable>()
                .HasMany(m => m.CastMembers)
                .WithMany(c => c.Movies)
                .Map(mc =>
                {
                    mc.ToTable("MovieCastMembers");
                    mc.MapLeftKey("MovieId");
                    mc.MapRightKey("CastMemberId");
                });
           
        }
    }
}