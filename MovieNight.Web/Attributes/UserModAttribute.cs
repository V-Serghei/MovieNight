﻿using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using MovieNight.BusinessLogic.Core;
using MovieNight.BusinessLogic.Interface;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities;
using MovieNight.Web.Infrastructure;

namespace MovieNight.Web.Attributes
{
    public class UserModAttribute: ActionFilterAttribute
    {
        private readonly ISession _sessionBL;

        public UserModAttribute()
        {
            var businessLogic = new BusinessLogic.BusinessLogic();
            _sessionBL = businessLogic.Session();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var apiCookie = HttpContext.Current.Request.Cookies["X-KEY"];
            if (apiCookie != null )
            {
                var agent = HttpContext.Current.Request.UserAgent;
                var profile = _sessionBL.GetUserByCookie(apiCookie.Value, agent);
                if (profile != null && profile.Role == LevelOfAccess.User)
                {
                    HttpContext.Current.SetMySessionObject(profile);
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new { controller = "Error", action = "Error404Page" }));
                }
            }
        }
    }
}