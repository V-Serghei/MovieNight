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
    public class NotificationController : Controller
    {
        // GET: Notification
        private readonly ISession _eventSession;

        public NotificationController()
        {
            var bl = new BusinessLogic.BusinessLogic();
            _eventSession = bl.Session();
        }
        [HttpPost]
        [Route("Notification/EventSave")]
        public ActionResult EventSave(EventDataModel eventDataModel)
        {
            EventE eventE = new EventE()
            {
                EventTitle = eventDataModel.EventTitle,
                EventDay = DateTime.Now
            };
            
            return RedirectToAction("Calendar");
        }
        public ActionResult Calendar()
        {
            return View();
        }
    }
}