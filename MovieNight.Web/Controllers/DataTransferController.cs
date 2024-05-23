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
        public ActionResult Inbox()
        {
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
        public ActionResult Read(int? mailId)
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<InboxD, InboxModel>();
            });
            var mapper = config.CreateMapper();
            var messageD = сompleteInbox.InboxRead(mailId);
            var message = mapper.Map<InboxModel>(messageD);
            return View(message);
        }
        public ActionResult Compose(int? id)
        {
            var model = new InboxModel
            {
                RecipientName = _session.GetUserData(id).Username
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult ComposeAdd(InboxModel model)
        {
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

        public ActionResult AddStarMail(int? mailId)
        {
            var sendMail = сompleteInbox.SetMailStar(mailId);
            return RedirectToAction("Starred");
        }
        public ActionResult DeleteStarMail(int? mailId)
        {
            var sendMail = сompleteInbox.DeleteMailStar(mailId);
            return RedirectToAction("Starred");
        }
        public ActionResult SentMail()
        {
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
        public ActionResult Trash(int? mailId)
        {
            var sendMail = сompleteInbox.DeleteMail(mailId);
            return RedirectToAction("Inbox");
        }
       
    }
}