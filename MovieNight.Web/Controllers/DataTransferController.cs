using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieNight.Web.Controllers
{
    public class DataTransferController : Controller
    {
        // GET: DataTransfer
        public ActionResult Inbox()
        {
            return View();
        }
        public ActionResult Read()
        {
            return View();
        }
        public ActionResult Compose()
        {
            return View();
        }
        public ActionResult Starred()
        {
            return View();
        }
        public ActionResult SentMail()
        {
            return View();
        }
        public ActionResult Trash()
        {
            return View();
        }
       
    }
}