using System.Collections.Generic;
using Microsoft.AspNet.Http;
using MovieNight.Web.Models.PersonalP.Bookmark;

using System.Collections.Generic;
using System.Linq;
using System.Web;
using HttpContext = System.Web.HttpContext;

namespace MovieNight.Web.Infrastructure
{
    public static class SessionExchanges
    {


        #region Bookmark
        /// <summary>
        /// Bookmark 
        /// </summary>
        /// <param name="current"></param>
        /// <returns>BookmarkTimeOf</returns>
        public static BookmarkTimeOf GetBookmarkTimeOf(this HttpContext current)
        {
            var bookmarkTimeOf = current?.Session["__BookmarkTimeOf"] as BookmarkTimeOf;
            bookmarkTimeOf?.SortByTimeAdd(); 
            return bookmarkTimeOf;
        }
        
        public static void SetBookmarkTimeOf(this HttpContext current, BookmarkTimeOf source)
        {
            current.Session.Add("__BookmarkTimeOf", source);
        }

        public static bool VerifyExistBookmark(this HttpContext current, BookmarkModel bookmarkToVerify)
        {
            bool exists = HttpContext.Current.GetBookmarkTimeOf().Bookmark.Any(b => b.IdMovie == bookmarkToVerify.IdMovie);
            return exists;
        }
        public static void RemoveBookmark(this HttpContext current, BookmarkModel bookmarkToRemove)
        {
            var bookmarkTimeOf = HttpContext.Current.GetBookmarkTimeOf();

            var existingBookmark = bookmarkTimeOf.Bookmark.FirstOrDefault(b => b.IdMovie == bookmarkToRemove.IdMovie);

            if (existingBookmark != null)
            {
                bookmarkTimeOf.Bookmark.Remove(existingBookmark);

                HttpContext.Current.SetBookmarkTimeOf(bookmarkTimeOf);
            }
        }


        #endregion
       
    }
}