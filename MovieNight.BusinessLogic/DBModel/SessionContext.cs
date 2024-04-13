using System.Data.Entity;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.BusinessLogic.DBModel
{
    public sealed class SessionContext:DbContext

    {
    public SessionContext() : base("name=MovieNight")
    {

    }


    public DbSet<SessionCookie> Sessions { get; set; }
    }
}