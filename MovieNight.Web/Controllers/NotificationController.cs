using Microsoft.Ajax.Utilities;
using MovieNight.BusinessLogic;
using MovieNight.BusinessLogic.Interface;
using MovieNight.Domain.Entities.Notification;
using MovieNight.Web.Models.Calendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieNight.Web.Controllers
{
    public class NotificationController : MasterController
    {
        // GET: Notification
        private readonly ISession EventSession;

        public NotificationController()
        {
            var bl = new BusinessLogic.BusinessLogic();
            EventSession = bl.Session();
        }
        [HttpPost]
        [Route("Notification/EventSave")]
        public ActionResult EventSave(EventDataModel eventDataModel)
        {
            EventE eventE = new EventE()
            {
                EventTitle = eventDataModel.title,
                Category = Domain.enams.EventColor.Blue,
                //StartTime = eventDataModel.beginning,
                //EndTime = eventDataModel.ending
            };
            return RedirectToAction("Index","MainPage");
        }
        public ActionResult Calendar()
        {
            return View();
        }
    }
}