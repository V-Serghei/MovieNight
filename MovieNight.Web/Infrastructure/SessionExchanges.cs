using System.Collections.Generic;
using Microsoft.AspNet.Http;
using MovieNight.Web.Models.PersonalP.Bookmark;

using System.Collections.Generic;
using System.Web;
using HttpContext = System.Web.HttpContext;

namespace MovieNight.Web.Infrastructure
{
    public static class SessionExchanges
    {
        public static ICollection<BookmarkTimeOf> GetBookmarkTimeOf(this HttpContext current)
        {
            return current?.Session["__ListBookmarkTimeOf"] as ICollection<BookmarkTimeOf>;
        }
        
        public static void SetBookmarkTimeOf(this HttpContext current, ICollection<BookmarkTimeOf> list)
        {
            current.Session.Add("__ListBookmarkTimeOf", list);
        }
    }
}