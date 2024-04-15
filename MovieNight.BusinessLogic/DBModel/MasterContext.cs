using System.Data.Entity;
using MovieNight.BusinessLogic.DBModel;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.MovieM.EfDbEntities;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Domain.Entities.PersonalP.PersonalPDb;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.BusinessLogic.DBModel
{
    public class MasterContext
    {
        public MovieContext MovieContext { get; }
        public SessionContext SessionContext { get; }
        public UserContext UserContext { get; }

        public MasterContext()
        {
            MovieContext = new MovieContext();
            SessionContext = new SessionContext();
            UserContext = new UserContext();
        }

        public DbSet<MovieDbTable> Movies => MovieContext.MovieDb;
        public DbSet<MovieCardDbTable> MovieCards => MovieContext.MovieCard;
        public DbSet<CastMemDbTable> CastMembers => MovieContext.CastDbTables;
        public DbSet<InterestingFactDbTable> InterestingFacts => MovieContext.InterestingFact;
        public DbSet<SessionCookie> Sessions => SessionContext.Sessions;
        public DbSet<UserDbTable> Users => UserContext.UsersT;
        public DbSet<PEdBdTable> PEdBdTables => UserContext.PEdBdTables;
        public DbSet<ViewListDbTable> ViewLists => UserContext.ViewList;

        public void SaveChanges()
        {
            MovieContext.SaveChanges();
            SessionContext.SaveChanges();
            UserContext.SaveChanges();
        }

    }
}