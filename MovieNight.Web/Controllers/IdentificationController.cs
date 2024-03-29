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
using System.Threading.Tasks;

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
        public async Task<ActionResult> LoginPost(LoginViewModel model)
        {
            
            LogInData logD = new LogInData
            {
                Email = model.Username,
                Password = model.Password,
                RememberMe = model.RememberMe,
                LoginTime = DateTime.Now,
                Ip = Request.ServerVariables["REMOTE_ADDR"]

            };
            UserVerification verification = await SessionUser.UserVerification(logD);
            if (verification.IsVerified == true)
            {
                SessionUser.SetUserSession(verification.UserId);
                return RedirectToAction("PersonalProfile", "InformationSynchronization");

            }
            else return View("Login",verification);
        }

        [HttpPost]
        public async Task<ActionResult> RegistPost(RegistViewModel rModel)
        {
            RegData RegD = new RegData
            {
                FullName = rModel.FullName,
                Password = rModel.Password,
                Email = rModel.Email,
                Checkbox = rModel.Checkbox,
                RegDateTime = DateTime.Now,
                Ip = Request.ServerVariables["REMOTE_ADDR"]

            };

            var rUserVerification = await SessionUser.UserAdd(RegD);

            if (rUserVerification.SuccessUniq == true)
            {
                
                if (SessionUser.UserСreation(RegD))
                {
                    SessionUser.SetUserSession(rUserVerification.UserId);
                    return RedirectToAction("PersonalProfile", "InformationSynchronization");
                }
                else
                {
                    rUserVerification.StatusMsg = "DATABASE ERROR";
                    return View("Register", rUserVerification); 
                }
            }
            else
            {
                return View("Register", rUserVerification); 
            }
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