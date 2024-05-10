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
using AutoMapper;
using MovieNight.Web.Infrastructure;

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
        [HttpGet]
        public ActionResult Inbox(int? userId)
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<InboxD, InboxModel>();
            });
            var mapper = config.CreateMapper();
            List<InboxD> messageD = сompleteInbox.InboxEquipment(userId);
            var message = mapper.Map<List<InboxModel>>(messageD);
            return View(message);
        }
        public ActionResult Read()
        {
            return View();
        }
        public ActionResult Compose()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ComposeAdd(InboxModel model)
        {
            var userId = System.Web.HttpContext.Current.GetMySessionObject().Id;
            var messageDb = new InboxD
            {
                Theme = model.Theme,
                Message = model.Message,
                RecipientName = model.RecipientName,
                Date = DateTime.Now,
                IsChecked = false,
                IsStarred = false,
                SenderName = System.Web.HttpContext.Current.GetMySessionObject().Username
            };
            var sendMail = сompleteInbox.SetAddMessage(messageDb);
            return RedirectToAction("SentMail");
        }
        public ActionResult Starred()
        {
            return View();
        }
        public ActionResult StarredList(int? userId)
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<InboxD, InboxModel>();
            });
            var mapper = config.CreateMapper();
            List<InboxD> messageD = сompleteInbox.InboxStarred(userId);
            var message = mapper.Map<List<InboxModel>>(messageD);
            return View(message);
        }
        public ActionResult SentMail()
        {
            return View();
        }
        public ActionResult SentMailList(int? userId)
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<InboxD, InboxModel>();
            });
            var mapper = config.CreateMapper();
            List<InboxD> messageD = сompleteInbox.InboxSent(userId);
            var message = mapper.Map<List<InboxModel>>(messageD);
            return View(message);
        }
        public ActionResult Trash()
        {
            return View();
        }
       
    }
}