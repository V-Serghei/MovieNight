using System.Data.Entity;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.BusinessLogic.DBModel
{
    public class SessionContext:DbContext

    {
    public SessionContext() : base("name=MovieNight")
    {

    }


    public virtual DbSet<SessionCookie> Sessions { get; set; }
    }
}