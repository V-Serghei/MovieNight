using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieNight.Web.Controllers
{
    public class NotificationController : Controller
    {
        // GET: Notification
        public ActionResult Calendar()
        {
            return View();
        }
    }
}