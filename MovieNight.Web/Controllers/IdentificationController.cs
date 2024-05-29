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
using AutoMapper;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.AchievementE;
using MovieNight.Web.Attributes;
using MovieNight.Web.Infrastructure;
using MovieNight.Web.Infrastructure.Different;
using MovieNight.Web.Models.Achievement;

namespace MovieNight.Web.Controllers
{
    public class IdentificationController : MasterController
    {

        #region Basic Settings

        /// <summary>
        /// Basic settings for the controller
        /// </summary>
        private readonly ISession _sessionUser;
        private readonly IMapper _mapper;
        private readonly IAchievements _achievements;
         
        public IdentificationController()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LogInData,UserModel >();
                cfg.CreateMap<AchievementE, AchievementModel>();
                cfg.CreateMap<AchievementModel, AchievementE>();


            });
            _mapper = config.CreateMapper();

            var sesControlBl = new BusinessLogic.BusinessLogic();
            _sessionUser = sesControlBl.Session();
            _achievements = sesControlBl.GetAchievementsService();
        }
        
        #endregion
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        [GuestMod]
        public async Task<JsonResult> LoginPost(LoginViewModel model)
        {
            var logD = new LogInData
            {
                Password = model.Password,
                RememberMe = model.RememberMe,
                LoginTime = DateTime.Now,
                Ip = Request.UserHostAddress,
                Agent = HttpContextInfrastructure.GetUserAgentInfo(Request)
            };
            if (ValidationStr.IsEmail(model.Username)) logD.Email = model.Username;
            else logD.Username = model.Username;
            var verification = await _sessionUser.UserVerification(logD);
            if (verification.IsVerified)
            {
                HttpCookie cookie = _sessionUser.GenCookie(verification.LogInData);
                ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                var us = _mapper.Map<UserModel>(verification.LogInData);
                System.Web.HttpContext.Current.SetMySessionObject(us);
                return Json(new { redirect = Url.Action("PersonalProfile", "InformationSynchronization") });

            }
            return Json(new { success = false, statusMsg = verification.StatusMsg });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [GuestMod]
        public async Task<JsonResult> RegistPost(RegistViewModel rModel)
        {
            if (rModel.Checkbox != "on")
            {
                return Json(new { success = false, statusMsg = "To register, you must agree to the license agreement!" });
            }
            var regD = new RegData
            {
                UserName = rModel.UserName,
                Password = rModel.Password,
                Email = rModel.Email,
                Checkbox = rModel.Checkbox=="on",
                RegDateTime = DateTime.Now,
                Ip = Request.UserHostAddress

            };

            var rUserVerification = await _sessionUser.UserAdd(regD);

            if (rUserVerification.SuccessUniq)
            {
                
                if (_sessionUser.UserСreation(regD))
                {
                    
                    rUserVerification.CurUser.Agent = HttpContextInfrastructure.GetUserAgentInfo(Request);
                    var cookie = _sessionUser.GenCookie(rUserVerification.CurUser);
                    ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                    var us = _mapper.Map<UserModel>(rUserVerification.CurUser);
                    if (_sessionUser != null) us.Id = (int)_sessionUser.GetIdCurrUser(us.Username);
                    System.Web.HttpContext.Current.SetMySessionObject(us);
                    var achievement = await _achievements.AchievementСheck((us.Id, AchievementType.Registration));
                    if (achievement != null)
                    {
                        var achiev = _mapper.Map<AchievementModel>(achievement);
                        System.Web.HttpContext.Current.SetListAchievement(achiev);
                    }
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
        [GuestMod]
        public ActionResult Login()
        {
            
            return View();
        }
        [HttpGet]
        [GuestMod]
        public ActionResult Register()
        {
            return View();
        }
        [HttpGet]
        [GuestMod]
        public ActionResult PagesRecoverpw()
        {
            return View();
        }

        [HttpGet]
        [GuestMod]
        public ActionResult Logout()
        {

            if(System.Web.HttpContext.Current.GetMySessionObject() != null){
                var delSession =
                    _sessionUser.DelSessionCurrUser(System.Web.HttpContext.Current.GetMySessionObject().Username);

                if (delSession)
                {
                    ControllerContext.HttpContext.Response.Cookies.Remove("X-KEY");
                    System.Web.HttpContext.Current.SetMySessionObject(null);
                    return RedirectToAction("Index", "MainPage");

                }
            }
            
            return View("Login");
        }
    }
}