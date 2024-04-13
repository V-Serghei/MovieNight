using MovieNight.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Deployment.Internal;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MovieNight.BusinessLogic.Interface;
using MovieNight.Domain.Entities.UserId;
using System.Reflection;
using System.Threading.Tasks;
using MovieNight.Web.Infrastructure;
using MovieNight.Web.Infrastructure.Different;

namespace MovieNight.Web.Controllers
{
    public class IdentificationController : Controller
    {
        private readonly ISession _sessionUser;
        
         
        public IdentificationController()
        {
            var sesControlBl = new BusinessLogic.BusinessLogic();
            _sessionUser = sesControlBl.Session();
        }

        [HttpPost]
        public async Task<JsonResult> LoginPost(LoginViewModel model)
        {
            var logD = new LogInData
            {
                Password = model.Password,
                RememberMe = model.RememberMe,
                LoginTime = DateTime.Now,
                Ip = Request.ServerVariables["REMOTE_ADDR"],
                Agent = HttpContextInfrastructure.GetUserAgentInfo(Request)
            };
            if (ValidationStr.IsEmail(model.Username)) logD.Email = model.Username;
            else logD.Username = model.Username;
            var verification = await _sessionUser.UserVerification(logD);
            if (verification.IsVerified)
            {
                HttpCookie cookie = _sessionUser.GenCookie(verification.LogInData);
                ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                return Json(new { redirect = Url.Action("PersonalProfile", "InformationSynchronization") });

            }
            return Json(new { success = false, statusMsg = verification.StatusMsg });
        }

        [HttpPost]
        public async Task<JsonResult> RegistPost(RegistViewModel rModel)
        {
            var regD = new RegData
            {
                UserName = rModel.UserName,
                Password = rModel.Password,
                Email = rModel.Email,
                Checkbox = rModel.Checkbox,
                RegDateTime = DateTime.Now,
                Ip = Request.UserHostAddress

            };

            var rUserVerification = await _sessionUser.UserAdd(regD);

            if (rUserVerification.SuccessUniq == true)
            {
                
                if (_sessionUser.UserСreation(regD))
                {
                    
                    var cookie = _sessionUser.GenCookie(rUserVerification.CurUser);
                    ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                    System.Web.HttpContext.Current.SetMySessionObject(rUserVerification.CurUser);
                    return Json(new { redirect = Url.Action("PersonalProfile", "InformationSynchronization") });
                }
                else
                {
                    rUserVerification.StatusMsg = "DATABASE ERROR";
                    return Json(new { success = false, statusMsg = rUserVerification.StatusMsg });
                }
            }
            else
            {
                return Json(new { success = false, statusMsg = rUserVerification.StatusMsg });
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