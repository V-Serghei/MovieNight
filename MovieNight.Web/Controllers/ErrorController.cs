using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieNight.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Error404Page()
        {
            return View();
        }
    }
}