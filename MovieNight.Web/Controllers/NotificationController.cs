using Microsoft.Ajax.Utilities;
using MovieNight.BusinessLogic;
using MovieNight.BusinessLogic.Interface;
using MovieNight.Domain.Entities.Notification;
using MovieNight.Web.Models.Calendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace MovieNight.Web.Controllers
{
    public class NotificationController : MasterController
    {
        // GET: Notification
        private readonly ISession _eventSession;

        public NotificationController()
        {
            var bl = new BusinessLogic.BusinessLogic();
            _eventSession = bl.Session();
        }
        
        [HttpPost]
        public JsonResult EventSave(CalendarEvent model)
        {
            
            if (model == null || string.IsNullOrEmpty(model.Title) || string.IsNullOrEmpty(model.Category))
            {
                return Json(new { success = false, message = "Title and Category are required." });
            }

            

            return Json(new { success = true, message = "Event saved successfully!" });
        }
        public ActionResult Calendar()
        {
            return View();
        }
    }
}