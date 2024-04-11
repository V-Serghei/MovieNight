using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieNight.Web.Controllers
{
    public class StatisticController : Controller
    {
        // GET: Statistic
        public ActionResult PersonalPromotionStatistics()
        {
            return View();
        }
    }
}