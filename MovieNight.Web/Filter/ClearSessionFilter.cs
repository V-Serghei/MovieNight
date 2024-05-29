using System.Web.Mvc;

namespace MovieNight.Web.Filter
{
    public class ClearSessionFilter: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var session = filterContext.HttpContext.Session;

            // session.Remove("__CurrentList");
            // session.Remove("__CurrentCommand");
            // session.Remove("__ListViewHSearch");
            // session.Remove("__CommandViewList");
            // session.Remove("__ListFilmSearch");
        }
    }
}