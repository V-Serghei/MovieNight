using System.Collections.Generic;
using Microsoft.AspNet.Http;
using MovieNight.Web.Models.PersonalP.Bookmark;

using System.Collections.Generic;
using System.Linq;
using System.Web;
using MovieNight.Web.Models.Achievement;
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

        #region AchievementModel

        public static List<AchievementModel> GetListAchievement(this HttpContext current)
        {
            return current?.Session["__Achievement"] as List<AchievementModel>;;
        }
        
        public static void SetListAchievement(this HttpContext current, AchievementModel source)
        {
            if (HttpContext.Current.GetListAchievement() != null)
            {
                var add =  HttpContext.Current.GetListAchievement();
                add.Add(source);
                current.Session.Add("__Achievement", add);
            }
            else
            {
                var newListA = new List<AchievementModel> { source };
                current.Session.Add("__Achievement", newListA);
            }
        }

        public static void RemoveAchievementFirst(this HttpContext current)
        {
            if (HttpContext.Current.GetListAchievement() != null)
            {
                var list = HttpContext.Current.GetListAchievement();
                list.RemoveAt(0);
            }
        }
        

        #endregion
       
    }
}