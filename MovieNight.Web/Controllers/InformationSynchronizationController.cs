using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace MovieNight.Web.Controllers
{
    public class InformationSynchronizationController : Controller
    {
        // GET: InformationSynchronization
        public ActionResult PersonalProfile()
        {
            return View();
        }

        public ActionResult UserTemplatePage()
        {
            return View();
        }

        public ActionResult MovieTemplatePage()
        {
            return View();
        }
    }
}