using MovieNight.BusinessLogic.Interface.IMail;
using MovieNight.BusinessLogic.Session.MailS;
using MovieNight.Domain.Entities;
using MovieNight.Domain.Entities.MailE;
using MovieNight.Web.Models;
using MovieNight.Web.Models.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MovieNight.Web.Controllers
{
    public class DataTransferController : Controller
    {
        internal IInbox сompleteInbox;
        public DataTransferController()
        {
            var MailBL = new BusinessLogic.BusinessLogic();
            сompleteInbox = MailBL.GetInbox();
        }
        // GET: DataTransfer
        public ActionResult Inbox()
        {
            List<InboxModel> Message = new List<InboxModel>();
            List<InboxD> MessageD = сompleteInbox.InboxEquipment();
            foreach (var TMP in MessageD) 
            {
                Message.Add(new InboxModel
                {
                    IsChecked = TMP.IsChecked,
                    SenderName = TMP.SenderName,
                    Theme = TMP.Theme,
                    Message = TMP.Message,
                    Date = new TimeModel
                    {
                        Year = TMP.Date.Year,
                        Month = TMP.Date.Month,
                        Day = TMP.Date.Day, 
                    },
                }
            );
            }
            return View(Message);
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