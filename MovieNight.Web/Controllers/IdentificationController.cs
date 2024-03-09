using MovieNight.Web.Models;
using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MovieNight.BusinessLogic.Interface;
using MovieNight.Domain.Entities.UserId;
using System.Reflection;

namespace MovieNight.Web.Controllers
{
    public class IdentificationController : Controller
    {
        internal ISession SessionUser;
        
         
        public IdentificationController()
        {
            var sesControlBl = new BusinessLogic.BusinessLogic();
            SessionUser = sesControlBl.Session();
        }
        [HttpPost]
        public ActionResult LoginPost(LoginViewModel model)
        {
            
            LogInData logD = new LogInData
            {
                Username = model.Username,
                Password = model.Password,
                RememberMe = model.RememberMe,
            };
            UserVerification verification =  SessionUser.UserVerification(logD);
            if (verification.IsVerified == true)
            {
                return RedirectToAction("PersonalProfile", "InformationSynchronization");

            }
            else return View("Login");
        }

        [HttpPost]
        public ActionResult RegistPost(RegistViewModel rModel)
        {
            RegData RegD = new RegData
            {
                Username = model.Username,
                Password = model.Password,
                RememberMe = model.RememberMe,
            };



            return View("Register");
        }

        // GET: Identification
        [HttpGet]
        public ActionResult Login()
        {
            
            return View();
        }
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [HttpGet]
        public ActionResult PagesRecoverpw()
        {
            return View();
        }
    }
}