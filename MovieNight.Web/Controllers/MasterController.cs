using System;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using MovieNight.BusinessLogic.Interface;
using MovieNight.Domain.Entities.UserId;
using MovieNight.Web.Infrastructure;
using MovieNight.Web.Models;

namespace MovieNight.Web.Controllers
{
    public class MasterController:Controller
    {
        private readonly ISession _session;
        private readonly IMapper _mapper;
        public MasterController()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LogInData,UserModel >();

            });
            _mapper = config.CreateMapper();
            var bl = new BusinessLogic.BusinessLogic();
            _session = bl.Session();
        }

        public void SessionStatus()
        {
            var apiCookie = Request.Cookies["X-KEY"];
            if (apiCookie != null)
            {
                var profile = _session.GetUserByCookie(apiCookie.Value,HttpContextInfrastructure.GetUserAgentInfo(Request));
                
                if (profile != null)
                {                var us = _mapper.Map<UserModel>(profile);

                    System.Web.HttpContext.Current.SetMySessionObject(us);
                    System.Web.HttpContext.Current.Session["LoginStatus"] = "login";
                }
                else
                {
                    System.Web.HttpContext.Current.Session.Clear();
                    if (ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("X-KEY"))
                    {
                        var cookie = ControllerContext.HttpContext.Request.Cookies["X-KEY"];
                        if (cookie != null)
                        {
                            cookie.Expires = DateTime.Now.AddDays(-1);
                            ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                        }
                    }

                    System.Web.HttpContext.Current.Session["LoginStatus"] = "logout";
                }
            }
            else
            {
                System.Web.HttpContext.Current.Session["LoginStatus"] = "logout";
            }
        }
    }
}