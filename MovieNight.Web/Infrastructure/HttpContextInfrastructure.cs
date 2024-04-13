

using System.Web;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.Web.Infrastructure
{
    public static class HttpContextInfrastructure
    {
        public static LogInData GetMySessionObject(this HttpContext current)
        {
            return (LogInData)current?.Session["__SessionObject"];
        }
    
        public static void SetMySessionObject(this HttpContext current, LogInData profile)
        {
            current.Session.Add("__SessionObject", profile);
        }
        public static string GetUserAgentInfo(HttpRequestBase request)
        {
            return request.Headers["User-Agent"];
        }

    }
}