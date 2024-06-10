

using System.Collections.Generic;
using System.Web;
using MovieNight.Domain.Entities.PersonalP.PersonalPDb;
using MovieNight.Web.Models;
using MovieNight.Web.Models.Movie;
using MovieNight.Web.Models.Movie.SearchPages;
using MovieNight.Web.Models.PersonalP.Bookmark;
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
        
        
        #region Sorting And Filtering
        
        public static void SetListToSession<T>(this HttpContext context, List<T> list)
        {
            context.Session["__CurrentList"] = list;
        }

        public static List<T> GetListFromSession<T>(this HttpContext context)
        {
            return context.Session["__CurrentList"] as List<T> ?? new List<T>();
        }
        
        
        public static void SetCommandState(this HttpContext context, ListSortCommand command)
        {
            context.Session["__CurrentCommand"] = command;
        }

        public static ListSortCommand GetCommandState(this HttpContext context)
        {
            return context.Session["__CurrentCommand"] as ListSortCommand;
        }
        
        
        public static bool CurrentCommandStateComparison(ListSortCommand command)
        {
             if (HttpContext.Current.GetCommandState() != null)
            {
                var commandS = HttpContext.Current.GetCommandState();
                if (command.Category == commandS.Category 
                    && command.Field == commandS.Field
                    && command.SearchParameter == commandS.SearchParameter
                    && command.SortingDirection == commandS.SortingDirection)
                    return true;
            }

            return false;
        }
        
        #endregion

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

        #region ListFilm
        

        public static MovieListModel GetListFilmS(this HttpContext current)
        {
            return current?.Session["__ListFilmSearch"] as MovieListModel;
        }
        
        public static void SetListFilmS(this HttpContext current, MovieListModel list)
        {
            current.Session.Add("__ListFilmSearch", list);
        }
        private const string FilmDictionaryKey = "__FilmPageDictionary";

        public static Dictionary<int, List<MovieTemplateInfModel>> GetFilmPageDictionary(this HttpContext current)
        {
            return current?.Session[FilmDictionaryKey] as Dictionary<int, List<MovieTemplateInfModel>> 
                   ?? new Dictionary<int, List<MovieTemplateInfModel>>();
        }

        public static void SetFilmPageDictionary(this HttpContext current, Dictionary<int, List<MovieTemplateInfModel>> dictionary)
        {
            current.Session[FilmDictionaryKey] = dictionary;
        }

        public static bool IsPageLoaded(this HttpContext current, int pageNumber)
        {
            var dictionary = current.GetFilmPageDictionary();
            return dictionary.ContainsKey(pageNumber);
        }

        public static List<MovieTemplateInfModel> GetPageFilms(this HttpContext current, int pageNumber)
        {
            var dictionary = current.GetFilmPageDictionary();
            return dictionary.ContainsKey(pageNumber) ? dictionary[pageNumber] : new List<MovieTemplateInfModel>();
        }

        public static void AddPageFilms(this HttpContext current, int pageNumber, List<MovieTemplateInfModel> films)
        {
            var dictionary = current.GetFilmPageDictionary();
            dictionary[pageNumber] = films;
            current.SetFilmPageDictionary(dictionary);
        }

        public static void ClearFilmPageDictionary(this HttpContext current)
        {
            current.Session.Remove(FilmDictionaryKey);
        }

        #endregion
        

        #endregion

    }
}