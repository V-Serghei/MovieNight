using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieNight.Web.Controllers
{
    public class SettingController : Controller
    {
        // GET: Setting
        public ActionResult ProfileConfiguration()
        {
            return View();
        }
    }
}