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
using Microsoft.AspNet.Http.Features;
using MovieNight.Web.Attributes;
using MovieNight.Web.Infrastructure;
using ISession = MovieNight.BusinessLogic.Interface.ISession;

namespace MovieNight.Web.Controllers
{
    public class DataTransferController : MasterController
    {
        internal IInbox сompleteInbox;
        internal ISession _session;
        public DataTransferController()
        {
            
            var BL = new BusinessLogic.BusinessLogic();
            сompleteInbox = BL.GetInbox();
            _session = BL.Session();
        }
        // GET: DataTransfer
        [HttpGet]
        [UserMod]
        public ActionResult Inbox()
        {
            SessionStatus();
            var userId = System.Web.HttpContext.Current.GetMySessionObject().Id;
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<InboxD, InboxModel>();
            });
            var mapper = config.CreateMapper();
            List<InboxD> messageD = сompleteInbox.InboxEquipment(userId);
            var message = mapper.Map<List<InboxModel>>(messageD);
            return View(message);
        }
        [HttpGet]
        [UserMod]
        public ActionResult Read(int? mailId)
        {
            SessionStatus();
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<InboxD, InboxModel>();
            });
            var mapper = config.CreateMapper();
            var messageD = сompleteInbox.InboxRead(mailId);
            var message = mapper.Map<InboxModel>(messageD);
            return View(message);
        }
        [UserMod]
        public ActionResult Compose(int? id)
        {
            SessionStatus();
            var model = new InboxModel
            {
                RecipientName = _session.GetUserData(id).Username
            };
            return View(model);
        }

        [HttpPost]
        [UserMod]
        public ActionResult ComposeAdd(InboxModel model)
        {
            SessionStatus();
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
        [HttpGet]
        [UserMod]
        public ActionResult Starred()
        {
            SessionStatus();
            var userId = System.Web.HttpContext.Current.GetMySessionObject().Id;
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<InboxD, InboxModel>();
            });
            var mapper = config.CreateMapper();
            List<InboxD> messageD = сompleteInbox.InboxStarred(userId);
            var message = mapper.Map<List<InboxModel>>(messageD);
            return View(message);
        }
        [UserMod]
        public ActionResult AddStarMail(int? mailId)
        {
            SessionStatus();
            var sendMail = сompleteInbox.SetMailStar(mailId);
            return RedirectToAction("Starred");
        }
        [HttpPost]
        [UserMod]
        public ActionResult DeleteStarMail(int? mailId)
        {
            SessionStatus();
            var sendMail = сompleteInbox.DeleteMailStar(mailId);
            return RedirectToAction("Starred");
        }
        [HttpGet]
        [UserMod]
        public ActionResult SentMail()
        {
            SessionStatus();
            var userId = System.Web.HttpContext.Current.GetMySessionObject().Id;
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<InboxD, InboxModel>();
            });
            var mapper = config.CreateMapper();
            List<InboxD> messageD = сompleteInbox.InboxSent(userId);
            var message = mapper.Map<List<InboxModel>>(messageD);
            return View(message);
        }
        [UserMod]
        public ActionResult Trash(int? mailId)
        {
            SessionStatus();
            var sendMail = сompleteInbox.DeleteMail(mailId);
            return RedirectToAction("Inbox");
        }
        [HttpGet]
        [UserMod]
        public ActionResult GetUnreadMessages()
        {
            SessionStatus();
            var userId = System.Web.HttpContext.Current.GetMySessionObject().Id;
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<InboxD, InboxModel>();
            });
            var mapper = config.CreateMapper();
            List<InboxD> messageD = сompleteInbox.InboxUnread(userId);
            var message = mapper.Map<List<InboxModel>>(messageD);
            return View(message);
        }
        [HttpGet]
        [UserMod]
        public JsonResult GetUnreadMessagesCount()
        {
            SessionStatus();
            var userId = System.Web.HttpContext.Current.GetMySessionObject().Id;
            var unreadMessagesCount = сompleteInbox.InboxUnread(userId).Count(m => !m.IsChecked);
            return Json(new { count = unreadMessagesCount }, JsonRequestBehavior.AllowGet);
        }
    }
}