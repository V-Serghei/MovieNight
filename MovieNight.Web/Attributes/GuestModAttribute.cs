using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using MovieNight.BusinessLogic.Core;
using MovieNight.BusinessLogic.Interface;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities;
using MovieNight.Domain.Entities.UserId;
using MovieNight.Web.Infrastructure;
using MovieNight.Web.Models;

namespace MovieNight.Web.Attributes
{
    public class GuestModAttribute : ActionFilterAttribute
    {
        private readonly ISession _sessionBL;
        private readonly IMapper _mapper;

        public GuestModAttribute()
        {
            var businessLogic = new BusinessLogic.BusinessLogic();
            _sessionBL = businessLogic.Session();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LogInData, UserModel>();
            });
            _mapper = config.CreateMapper();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var apiCookie = HttpContext.Current.Request.Cookies["X-KEY"];
            UserModel us = null;
            
            if (apiCookie != null)
            {
                var agent = HttpContext.Current.Request.UserAgent;
                var profile = _sessionBL.GetUserByCookie(apiCookie.Value, agent);
                us = _mapper.Map<UserModel>(profile);

                if (profile == null || (profile.Role != LevelOfAccess.Guest && profile.Role != LevelOfAccess.User && profile.Role != LevelOfAccess.Admin && profile.Role != LevelOfAccess.Moderator))
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new { controller = "Error", action = "Error404Page" }));
                    return;
                }
            }
            
            HttpContext.Current.SetMySessionObject(us);
        }
    }
}