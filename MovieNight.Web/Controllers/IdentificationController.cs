using MovieNight.Web.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MovieNight.BusinessLogic.Interface;
using MovieNight.Domain.Entities.UserId;
using System.Threading.Tasks;
using AutoMapper;
using MovieNight.BusinessLogic.DBModel;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.AchievementE;
using MovieNight.Helpers.CryptographyH;
using MovieNight.Web.Attributes;
using MovieNight.Web.Infrastructure;
using MovieNight.Web.Infrastructure.Different;
using MovieNight.Web.Models.Achievement;
using MovieNight.Web.Models.PersonalP.RecoverM;

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
        
        #region Login
        
        /// <summary>
        /// Login page for the user
        /// Login is a guest mod
        /// Login get and post methods
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [GuestMod]
        public async Task<JsonResult> LoginPost(LoginViewModel model)
        {
            if (!ModelState.IsValid) return Json(new
            {
                success = false, 
                errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
            });
            string rememberMe = "off";
            if (model.RememberMe == "on")
                rememberMe = "on";
            
            var logD = new LogInData
            {
                Password = model.Password,
                RememberMe = rememberMe == "on",
                LoginTime = DateTime.Now,
                Ip = Request.UserHostAddress,
                Agent = HttpContextInfrastructure.GetUserAgentInfo(Request)
            };
            if (ValidationStr.IsEmail(model.Username)) logD.Email = model.Username;
            else logD.Username = model.Username;
            var verification = await _sessionUser.UserVerification(logD);
            if (verification.IsVerified)
            {   
                HttpCookie cookie = rememberMe == "on"? _sessionUser.GenCookieLongTime(verification.LogInData) : 
                    _sessionUser.GenCookie(verification.LogInData);
                ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                var us = _mapper.Map<UserModel>(verification.LogInData);
                System.Web.HttpContext.Current.SetMySessionObject(us);
                return Json(new { redirect = Url.Action("PersonalProfile", "InformationSynchronization") });

            }
            return Json(new { success = false, statusMsg = verification.StatusMsg });
        }
        
        [HttpGet]
        [GuestMod]
        public ActionResult Login()
        {
            return View();
        }
        
        #endregion

        #region Registeration
        
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
        
        [HttpGet]
        [GuestMod]
        public ActionResult Register()
        {
            return View();
        }
        
        #endregion

        #region Recover Password

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RecoverPassword(RecoverPasswordViewModel rModel)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid email address" });
            }

            var code = new Random().Next(100000, 999999).ToString();

            HttpContext.Session["RecoveryCode"] = code;
            HttpContext.Session["RecoveryEmail"] = rModel.Email;

            SendEmail.SendRecoveryEmail(rModel.Email, code);
            return Json(new { success = true, message = "Recovery email sent" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult VerifyCode(VerifyCodeViewModel vModel)
        {
            var storedCode = HttpContext.Session["RecoveryCode"] as string;
            var storedEmail = HttpContext.Session["RecoveryEmail"] as string;

            if (vModel.Code == storedCode)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Invalid code" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> VerifyCodeAndResetPassword(ResetPasswordViewModel model)
        {
            var storedEmail = HttpContext.Session["RecoveryEmail"] as string;
            
            var newPasswordResult = await _sessionUser.UserResetPassword(storedEmail, model.NewPassword);

            if(newPasswordResult!=null){
                if (newPasswordResult.Success)
                {
                    HttpContext.Session.Remove("RecoveryCode");
                    HttpContext.Session.Remove("RecoveryEmail");
                    return Json(new { success = true, message = newPasswordResult.Message });

                }
                else
                {
                    return Json(new { success = false, message = newPasswordResult.Message });

                }
            }
            
            return Json(new { success = false, message = "Invalid code or email" });
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        [GuestMod]
        public JsonResult ResetPassword(ResetPasswordViewModel rModel)
        {
            var storedCode = HttpContext.Session["RecoveryCode"] as string;
            var storedEmail = HttpContext.Session["RecoveryEmail"] as string;

            if (rModel.Code == storedCode && rModel.Email == storedEmail)
            {
                using (var db = new UserContext())
                {
                    var user = db.UsersT.FirstOrDefault(u => u.Email == rModel.Email);
                    if (user != null)
                    {
                        user.Password = HashPassword.HashPass(rModel.NewPassword, user.Salt);
                        db.SaveChanges();

                        HttpContext.Session.Remove("RecoveryCode");
                        HttpContext.Session.Remove("RecoveryEmail");

                        return Json(new { success = true, message = "Password reset successfully" });
                    }
                }
            }

            return Json(new { success = false, message = "Invalid code or email" });
        }
        public ActionResult VerifyCode()
        {
            return View();
        }
        [HttpGet]
        [GuestMod]
        public ActionResult PagesRecoverPassword()
        {
            return View();
        }

        #endregion
        
        #region Logout
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
        #endregion
    }
}