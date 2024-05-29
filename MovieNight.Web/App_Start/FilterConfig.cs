using System.Web.Mvc;
using MovieNight.Web.Attributes;
using MovieNight.Web.Filter;

namespace MovieNight.Web
{
    public abstract class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ClearSessionFilter()); 
            
        }
    }
}