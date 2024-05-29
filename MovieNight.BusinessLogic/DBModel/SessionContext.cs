using System.Data.Entity;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.BusinessLogic.DBModel
{
    public sealed class SessionContext:DbContext

    {
    public SessionContext() : base("name=MovieNight")
    {
        this.Database.CommandTimeout = 180;
    }


    public DbSet<SessionCookie> Sessions { get; set; }
    }
}