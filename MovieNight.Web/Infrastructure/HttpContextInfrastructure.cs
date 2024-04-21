

using System.Collections.Generic;
using System.Web;
using MovieNight.Domain.Entities.UserId;
using MovieNight.Web.Models;
using MovieNight.Web.Models.Movie;
using MovieNight.Web.Models.SortingSearchingFiltering;

namespace MovieNight.Web.Infrastructure
{
    public static class HttpContextInfrastructure
    {
        public static UserModel GetMySessionObject(this HttpContext current)
        {
            return (UserModel)current?.Session["__SessionObject"];
        }
        
        public static void SetMySessionObject(this HttpContext current, UserModel profile)
        {
            current.Session.Add("__SessionObject", profile);
        }
        public static void SerGlobalParam(int? id)
        {
            HttpContext.Current.Session["UserId"] = id;
        }
        
        public static int? GetGlobalParam()
        {
            return (int?)HttpContext.Current.Session["UserId"];
        }
        public static string GetUserAgentInfo(HttpRequestBase request)
        {
            return request.Headers["User-Agent"];
        }

        #region CACHE

        /// <summary>
        /// Temporary storage of user action results for optimization
        /// by reducing the number of queries in the database
        /// </summary>


        #region ViewList
        //################################
        //Regarding the view list
        //###############################
        public static List<ViewingHistoryModel> GetListViewingHistoryS(this HttpContext current)
        {
            return current?.Session["__ListViewHSearch"] as List<ViewingHistoryModel>;
        }
        
        public static void SetListViewingHistoryS(this HttpContext current, List<ViewingHistoryModel> list)
        {
            current.Session.Add("__ListViewHSearch", list);
        }
        public static ViewListSort GetCommandViewList(this HttpContext current)
        {
            return current?.Session["__CommandViewList"] as ViewListSort;
        }
        
        public static void SetCommandViewList(this HttpContext current, ViewListSort command)
        {
            current.Session.Add("__CommandViewList", command);
        }

        public static bool CurrentCommandStateComparison(ViewListSort command)
        {
            if (HttpContext.Current.GetCommandViewList() != null)
            {
                var commandS = HttpContext.Current.GetCommandViewList();
                if (command.Category == commandS.Category 
                    && command.Field == commandS.Field
                    && command.SearchParameter == commandS.SearchParameter
                    && command.SortingDirection == commandS.SortingDirection)
                    return true;
            }

            return false;
        }
        

        
        #endregion
       
        
        

        #endregion

    }
}