using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MovieNight.BusinessLogic.Interface;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.Web.Controllers
{
    public class MainPageController : Controller
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



            return View();
        }

        public ActionResult Primary()
        {

            return View();
        }

        public ActionResult News()
        {
            return View();
        }

        public ActionResult Top()
        {
            return View();
        }

        public ActionResult AreWatching()
        {
            return View();
        }
    }
}