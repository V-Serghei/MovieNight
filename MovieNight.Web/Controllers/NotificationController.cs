using Microsoft.Ajax.Utilities;
using MovieNight.BusinessLogic;
using MovieNight.BusinessLogic.Interface;
using MovieNight.Web.Models.Calendar;
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
        private readonly ISession EventSession;

        public NotificationController()
        {
            var bl = new BusinessLogic.BusinessLogic();
            EventSession = bl.Session();
        }


        [HttpPost]
        public ActionResult EventSave(EventDataModel eventDataModel)
        {
            if (eventDataModel == null)
            {
                return View("Calendar");
            }
            //EventE eventE = new EventE()
            //{
            //    EventTitle = eventDataModel.title,
            //    Category = Domain.enams.EventColor.Blue,
            //    StartTime = eventDataModel.beginning,
            //    EndTime = eventDataModel.ending
            //};
            return RedirectToAction("UploadingTheFileOfAddingMoviesDb","Admin");
        }

        [HttpGet]
        public ActionResult Calendar()
        {
            return View();
        }
    }
}