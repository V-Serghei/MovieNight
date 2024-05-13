using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Microsoft.AspNet.Http.Features;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.BusinessLogic.Session.Service;
using MovieNight.Domain.Entities.AchievementE;
using MovieNight.Domain.Entities.UserId;
using MovieNight.Web.Infrastructure;
using MovieNight.Web.Models;
using MovieNight.Web.Models.Achievement;
using ISession = MovieNight.BusinessLogic.Interface.ISession;

namespace MovieNight.Web.Controllers
{
    public class StatisticController : MasterController
    {
        #region Common Elements

        private readonly ISession _sessionUser;
        private readonly IMovie _movie;
        private readonly IMapper _mapper;
        private readonly IAchievements _achievements;


        public StatisticController()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AchievementE, AchievementModel>();
                cfg.CreateMap<AchievementModel, AchievementE>();


            });
            _mapper = config.CreateMapper();

            var sesControlBl = new BusinessLogic.BusinessLogic();
            _sessionUser = sesControlBl.Session();
            _achievements = sesControlBl.GetAchievementsService();
            _movie = sesControlBl.GetMovieService();
        }
        

        #endregion
        
        
        
        
        // GET: Statistic
        
        
        
        public ActionResult PersonalPromotionStatistics()
        {
            var userId = System.Web.HttpContext.Current.GetMySessionObject()?.Id;
            var dataStatistic = _movie.GetDataStatisticPage(userId);
            
            
            return View();
        }

        public async Task<JsonResult> GetChartDataRating()
        {
            var user = System.Web.HttpContext.Current.GetMySessionObject();
            if (user != null)
            {
                var data = await _movie.GetInfOnFilmScores(user.Id);

                if(data!=null){
                    return Json(new
                    {
                        mygrade = data.MyGrades,
                        movieNightGrade = data.MovieNightGrade,
                        title = data.TitleMovie,
                        idMovie = data.IdMovie,
                        timeAdd = data.DataAddGrade
                    },JsonRequestBehavior.AllowGet);
                }
            }
            
            return Json(null);

        }
    }
}