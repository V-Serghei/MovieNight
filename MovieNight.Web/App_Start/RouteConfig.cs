using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MovieNight.Web
{
    
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "MainPage", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "MovieTemplate",
                url: "InformationSynchronization/MovieTemplatePage/{id}",
                defaults: new { controller = "InformationSynchronization", action = "MovieTemplatePage", id = UrlParameter.Optional }
            );
            

            routes.MapRoute(
                name: "ClearBookmarks",
                url: "InformationSynchronization/ClearBookmarks",
                defaults: new { controller = "InformationSynchronization", action = "ClearBookmarks" }
            );
            
            routes.MapRoute(
                name: "SearchMovies",
                url: "{controller}/{action}/{searchTerm}",
                defaults: new { controller = "SearchSortAdd", action = "SearchMovies", searchTerm = UrlParameter.Optional }
            );


            
        }
    }
}
