using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MovieNight.BusinessLogic.Interface;
using MovieNight.Domain.Entities.UserId;
using MovieNight.Web.Infrastructure;

namespace MovieNight.Web.Controllers
{
    public class MainPageController : MasterController
    {
        internal ISession SessionUser;


        public MainPageController()
        {
            var sesControlBl = new BusinessLogic.BusinessLogic();
            SessionUser = sesControlBl.Session();
        }
        // GET: MainPage
        public ActionResult Index()
        {
            SessionStatus();
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] != "login")
            {
                return RedirectToAction("Login", "Identification");
            }
            var user = System.Web.HttpContext.Current.GetMySessionObject();
            HttpContextInfrastructure.SerGlobalParam(SessionUser.GetIdCurrUser(user.Username));
            return View();
        }

        public ActionResult Primary()
        {
            SessionStatus();
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] != "login")
            {
                return RedirectToAction("Login", "Identification");
            }
            var user = System.Web.HttpContext.Current.GetMySessionObject();
            return View();
        }

        public ActionResult News()
        {
            SessionStatus();
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] != "login")
            {
                return RedirectToAction("Login", "Identification");
            }
            var user = System.Web.HttpContext.Current.GetMySessionObject();
            return View();
        }

        public ActionResult Top()
        {
            SessionStatus();
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] != "login")
            {
                return RedirectToAction("Login", "Identification");
            }
            var user = System.Web.HttpContext.Current.GetMySessionObject();
            return View();
        }

        public ActionResult AreWatching()
        {
            SessionStatus();
            if ((string)System.Web.HttpContext.Current.Session["LoginStatus"] != "login")
            {
                return RedirectToAction("Login", "Identification");
            }
            var user = System.Web.HttpContext.Current.GetMySessionObject();
            return View();
        }
    }
}